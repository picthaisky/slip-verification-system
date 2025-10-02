# Message Queue System Documentation

## Overview

The Message Queue System is built on RabbitMQ 3.12+ and provides asynchronous message processing for slip verification, notifications, and report generation. It implements enterprise-grade patterns including retry mechanisms with exponential backoff, dead letter queues (DLQ), and health monitoring.

## Architecture

### Components

1. **RabbitMQ Connection Factory** - Manages connections to RabbitMQ with automatic recovery
2. **Queue Setup** - Declares queues, exchanges, and bindings on startup
3. **Message Publisher** - Publishes messages to queues
4. **Consumers** - Background services that process messages from queues
5. **Health Checks** - Monitors RabbitMQ connection status

### Design Patterns

- **Producer/Consumer Pattern** - Decouples message producers from consumers
- **Retry Pattern** - Automatic retry with exponential backoff (2^retryCount seconds)
- **Dead Letter Queue (DLQ)** - Failed messages after max retries are sent to DLQ
- **Circuit Breaker** - Connection recovery and health monitoring

## Queue Definitions

### Queues

| Queue Name | Purpose | Routing Key | Max Retries |
|------------|---------|-------------|-------------|
| `slip-processing-queue` | Process slip OCR and verification | `slip.*` | 3 |
| `notifications-queue` | General notifications | `notification.*` | 3 |
| `email-notifications-queue` | Email notifications | `notification.email` | 3 |
| `push-notifications-queue` | Push notifications | `notification.push` | 3 |
| `reports-queue` | Report generation | `report.generation` | 3 |
| `dead-letter-queue` | Failed messages | `#` | N/A |

### Exchanges

| Exchange Name | Type | Description |
|---------------|------|-------------|
| `slip-verification-exchange` | Topic | Main exchange for routing messages |
| `dead-letter-exchange` | Direct | Exchange for dead letter queue |

## Configuration

### appsettings.json

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "RetryCount": 3,
    "RetryDelaySeconds": 5
  }
}
```

### Environment Variables (Production)

```bash
RabbitMQ__HostName=rabbitmq.production.com
RabbitMQ__Port=5672
RabbitMQ__Username=slip_verification_user
RabbitMQ__Password=<secure-password>
RabbitMQ__VirtualHost=/slip-verification
```

## Service Registration

### Startup.cs / Program.cs

```csharp
using SlipVerification.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Message Queue services
builder.Services.AddMessageQueueServices(builder.Configuration);

var app = builder.Build();

// Initialize queues on startup
app.Services.InitializeMessageQueues();

app.Run();
```

## Usage Examples

### 1. Publishing Messages

#### Slip Processing

```csharp
using SlipVerification.Application.Interfaces.MessageQueue;
using SlipVerification.Application.DTOs.MessageQueue;

public class SlipController : ControllerBase
{
    private readonly IMessagePublisher _publisher;

    public SlipController(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessSlip(SlipUploadDto dto)
    {
        var message = new SlipProcessingMessage
        {
            SlipId = Guid.NewGuid(),
            UserId = User.GetUserId(),
            ImageUrl = dto.ImageUrl,
            ProcessingType = "OCR"
        };

        // Publish to slip processing queue
        await _publisher.PublishAsync(QueueNames.SlipProcessing, message);

        return Accepted(new { message = "Slip queued for processing" });
    }
}
```

#### Notification

```csharp
var notificationMessage = new NotificationQueueMessage
{
    UserId = userId,
    Channel = "Email",
    Title = "Payment Received",
    Message = "Your payment has been confirmed",
    Priority = 2, // High priority
    Data = new Dictionary<string, object>
    {
        { "TransactionId", transactionId },
        { "Amount", amount }
    }
};

await _publisher.PublishAsync(QueueNames.Notifications, notificationMessage);
```

#### Report Generation

```csharp
var reportMessage = new ReportGenerationMessage
{
    ReportId = Guid.NewGuid(),
    UserId = userId,
    ReportType = "Monthly",
    StartDate = DateTime.UtcNow.AddMonths(-1),
    EndDate = DateTime.UtcNow,
    Parameters = new Dictionary<string, object>
    {
        { "IncludeDetails", true },
        { "Format", "PDF" }
    }
};

await _publisher.PublishAsync(QueueNames.Reports, reportMessage);
```

### 2. Publishing with Exchange and Routing Key

```csharp
await _publisher.PublishAsync(
    ExchangeNames.SlipVerification,
    RoutingKeys.SlipVerified,
    message
);
```

## Message Processing

### Consumer Implementation

All consumers inherit from `BaseRabbitMQConsumer` which provides:
- Automatic retry with exponential backoff
- Dead letter queue handling
- Error logging
- Graceful shutdown

#### Custom Consumer Example

```csharp
public class MyCustomConsumer : BaseRabbitMQConsumer
{
    protected override string QueueName => "my-custom-queue";
    protected override int PrefetchCount => 10;
    protected override int MaxRetryCount => 3;

    public MyCustomConsumer(
        IRabbitMQConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ILogger<MyCustomConsumer> logger)
        : base(connectionFactory, serviceProvider, logger)
    {
    }

    protected override async Task ProcessMessageAsync(
        string message, 
        CancellationToken cancellationToken)
    {
        // Deserialize message
        var myMessage = JsonSerializer.Deserialize<MyMessage>(message);
        
        // Process message
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IMyService>();
        await service.ProcessAsync(myMessage, cancellationToken);
    }
}
```

### Register Custom Consumer

```csharp
services.AddHostedService<MyCustomConsumer>();
```

## Retry Mechanism

### Exponential Backoff Strategy

| Retry Attempt | Delay |
|---------------|-------|
| 1st retry | 2 seconds (2^1) |
| 2nd retry | 4 seconds (2^2) |
| 3rd retry | 8 seconds (2^3) |
| After max retries | Send to DLQ |

### Retry Headers

Messages include the following headers during retry:
- `x-retry-count` - Number of retry attempts
- `x-first-death-reason` - Original error message

## Dead Letter Queue (DLQ)

### Monitoring Failed Messages

Messages that exceed the maximum retry count are sent to the `dead-letter-queue`. These messages can be:

1. **Inspected** - View error details and message content
2. **Requeued** - Manually republish to original queue after fixing issues
3. **Discarded** - Remove permanently if not recoverable

### RabbitMQ Management UI

Access at: `http://localhost:15672` (default credentials: guest/guest)

Navigate to **Queues** → **dead-letter-queue** to view failed messages.

## Health Checks

### Endpoint

```
GET /health
```

### Response

```json
{
  "status": "Healthy",
  "checks": {
    "rabbitmq": {
      "status": "Healthy",
      "description": "RabbitMQ connection is healthy"
    }
  }
}
```

### Integration with Monitoring

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<RabbitMQHealthCheck>("rabbitmq", tags: new[] { "ready", "messaging" });

app.MapHealthChecks("/health");
```

## Performance & Scalability

### Configuration Options

#### Prefetch Count
Controls how many messages a consumer fetches at once:

```csharp
protected override int PrefetchCount => 10; // Default
```

- Lower values: Better load distribution
- Higher values: Better throughput for fast consumers

#### Consumer Count
Scale horizontally by running multiple consumer instances:

```csharp
// In multiple containers/pods
services.AddHostedService<SlipProcessingConsumer>(); // Instance 1
// RabbitMQ automatically load balances across consumers
```

### Monitoring Metrics

Key metrics to monitor:
- Queue depth (messages waiting)
- Consumer throughput (messages/second)
- Message processing time
- Retry rate
- DLQ message count

## Production Deployment

### Docker Compose

```yaml
version: '3.8'

services:
  rabbitmq:
    image: rabbitmq:3.12-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: slip_verification_user
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
      RABBITMQ_DEFAULT_VHOST: /slip-verification
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 30s
      timeout: 10s
      retries: 5

volumes:
  rabbitmq_data:
```

### Kubernetes

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: rabbitmq-config
data:
  RABBITMQ__HOSTNAME: rabbitmq-service
  RABBITMQ__PORT: "5672"
  RABBITMQ__USERNAME: slip_verification_user
  RABBITMQ__VIRTUALHOST: /slip-verification
---
apiVersion: v1
kind: Secret
metadata:
  name: rabbitmq-secret
type: Opaque
stringData:
  RABBITMQ__PASSWORD: <base64-encoded-password>
```

## Error Handling

### Consumer Error Handling

```csharp
protected override async Task ProcessMessageAsync(string message, CancellationToken ct)
{
    try
    {
        // Process message
    }
    catch (TransientException ex)
    {
        // Will be retried automatically
        Logger.LogWarning(ex, "Transient error, will retry");
        throw;
    }
    catch (PermanentException ex)
    {
        // Log and skip (will go to DLQ after max retries)
        Logger.LogError(ex, "Permanent error");
        throw;
    }
}
```

### Circuit Breaker

RabbitMQ connection includes automatic recovery:
- Network recovery interval: 10 seconds
- Heartbeat: 60 seconds
- Connection attempts: Unlimited with exponential backoff

## Best Practices

### 1. Message Design
- Keep messages small and focused
- Include correlation IDs for tracing
- Add timestamps for debugging
- Use DTOs for type safety

### 2. Idempotency
- Design consumers to handle duplicate messages
- Use unique message IDs to detect duplicates
- Store processing state in database

### 3. Monitoring
- Set up alerts for queue depth > 1000
- Monitor DLQ for failed messages
- Track consumer lag
- Monitor RabbitMQ health

### 4. Testing
```csharp
[Fact]
public async Task Should_Publish_Message_Successfully()
{
    // Arrange
    var mockConnection = new Mock<IConnection>();
    var mockChannel = new Mock<IModel>();
    // ... setup mocks
    
    var publisher = new RabbitMQPublisher(connectionFactory, logger);
    
    // Act
    await publisher.PublishAsync(QueueNames.SlipProcessing, message);
    
    // Assert
    mockChannel.Verify(x => x.BasicPublish(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<IBasicProperties>(),
        It.IsAny<byte[]>()), Times.Once);
}
```

### 5. Security
- Use strong passwords for RabbitMQ
- Limit network access to RabbitMQ port
- Use TLS for connections in production
- Implement message encryption for sensitive data

## Troubleshooting

### Common Issues

#### 1. Connection Failed
```
Error: Connection to RabbitMQ failed
```
**Solution:** 
- Check RabbitMQ is running: `docker ps | grep rabbitmq`
- Verify credentials in appsettings.json
- Check network connectivity

#### 2. Messages Stuck in Queue
```
Queue depth increasing, no consumers
```
**Solution:**
- Check consumer logs for errors
- Verify consumers are registered as hosted services
- Check if consumers are processing too slowly

#### 3. High DLQ Rate
```
Many messages ending up in dead-letter-queue
```
**Solution:**
- Review error logs for common failures
- Check if retry count is too low
- Verify message format matches consumer expectations

### Debug Mode

Enable detailed logging:

```json
{
  "Logging": {
    "LogLevel": {
      "SlipVerification.Infrastructure.MessageQueue": "Debug"
    }
  }
}
```

## Future Enhancements

- [ ] Message priority queues
- [ ] Scheduled message delivery
- [ ] Message deduplication
- [ ] Saga pattern support
- [ ] Message compression
- [ ] Schema validation
- [ ] Distributed tracing integration (OpenTelemetry)
- [ ] Rate limiting per queue
- [ ] Auto-scaling based on queue depth

## References

- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [RabbitMQ .NET Client](https://www.rabbitmq.com/dotnet-api-guide.html)
- [Message Queue Patterns](https://www.enterpriseintegrationpatterns.com/)

## Support

For issues or questions:
1. Check RabbitMQ logs: `docker logs rabbitmq-container`
2. Review application logs in `Logs/` directory
3. Check RabbitMQ Management UI at `http://localhost:15672`
4. Verify health endpoint: `curl http://localhost:5000/health`
