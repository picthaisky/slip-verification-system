# Message Queue System - Quick Start Guide

## Prerequisites

- .NET 9 SDK
- Docker and Docker Compose
- RabbitMQ 3.12+ (or use Docker)

## Step 1: Start RabbitMQ

### Using Docker

```bash
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  rabbitmq:3.12-management
```

### Using Docker Compose

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  rabbitmq:
    image: rabbitmq:3.12-management
    container_name: rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq

volumes:
  rabbitmq_data:
```

Start:
```bash
docker-compose up -d
```

Verify RabbitMQ is running:
- Management UI: http://localhost:15672 (guest/guest)

## Step 2: Configure Application

Update `appsettings.json`:

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

## Step 3: Register Services

In `Program.cs`:

```csharp
using SlipVerification.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ... other services

// Add Message Queue services
builder.Services.AddMessageQueueServices(builder.Configuration);

var app = builder.Build();

// Initialize queues on startup
app.Services.InitializeMessageQueues();

// ... other middleware

app.Run();
```

## Step 4: Publish Your First Message

### Example Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using SlipVerification.Application.Interfaces.MessageQueue;
using SlipVerification.Application.DTOs.MessageQueue;
using SlipVerification.Infrastructure.MessageQueue;

namespace SlipVerification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestQueueController : ControllerBase
{
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<TestQueueController> _logger;

    public TestQueueController(
        IMessagePublisher publisher,
        ILogger<TestQueueController> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    [HttpPost("slip")]
    public async Task<IActionResult> PublishSlipMessage()
    {
        var message = new SlipProcessingMessage
        {
            SlipId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ImageUrl = "https://example.com/slip.jpg",
            ProcessingType = "OCR"
        };

        await _publisher.PublishAsync(QueueNames.SlipProcessing, message);

        return Ok(new 
        { 
            message = "Message published successfully",
            messageId = message.MessageId
        });
    }

    [HttpPost("notification")]
    public async Task<IActionResult> PublishNotificationMessage()
    {
        var message = new NotificationQueueMessage
        {
            UserId = Guid.NewGuid(),
            Channel = "Email",
            Title = "Test Notification",
            Message = "This is a test notification",
            Priority = 1
        };

        await _publisher.PublishAsync(QueueNames.Notifications, message);

        return Ok(new 
        { 
            message = "Notification published successfully",
            messageId = message.MessageId
        });
    }
}
```

## Step 5: Run the Application

```bash
cd slip-verification-api/src/SlipVerification.API
dotnet run
```

## Step 6: Test the Message Queue

### Publish a Message

```bash
curl -X POST http://localhost:5000/api/testqueue/slip \
  -H "Content-Type: application/json"
```

### Monitor in RabbitMQ Management UI

1. Open http://localhost:15672
2. Login with `guest/guest`
3. Go to **Queues** tab
4. You should see:
   - `slip-processing-queue`
   - `notifications-queue`
   - `email-notifications-queue`
   - `push-notifications-queue`
   - `reports-queue`
   - `dead-letter-queue`

### Check Consumer Logs

The consumers run as background services and will automatically process messages:

```
[12:00:00 INF] Consumer started for queue slip-processing-queue
[12:00:01 INF] Processing message from queue slip-processing-queue
[12:00:03 INF] Processing slip abc123... for user xyz789...
[12:00:05 INF] Slip abc123... processed successfully
[12:00:05 INF] Message processed successfully from queue slip-processing-queue
```

## Step 7: Verify Health Check

```bash
curl http://localhost:5000/health
```

Expected response:
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

## Common Commands

### Check Queue Status

```bash
# Using RabbitMQ CLI (in container)
docker exec rabbitmq rabbitmqctl list_queues

# Expected output:
# slip-processing-queue     0
# notifications-queue       0
# reports-queue            0
# dead-letter-queue        0
```

### Purge Queue (Development Only)

```bash
docker exec rabbitmq rabbitmqctl purge_queue slip-processing-queue
```

### View Consumer Count

In RabbitMQ Management UI:
- Navigate to **Queues**
- Check the **Consumers** column for each queue

## Testing Different Scenarios

### 1. Test Normal Processing

```bash
curl -X POST http://localhost:5000/api/testqueue/slip
```

Check logs - should process successfully.

### 2. Test Retry Mechanism

Temporarily stop your database or cause an error in the consumer. The message will be retried with exponential backoff.

### 3. Test Dead Letter Queue

Let a message fail 3 times (max retries). It will be sent to `dead-letter-queue`.

## Troubleshooting

### RabbitMQ Connection Error

```
Error: Connection to RabbitMQ failed
```

**Check:**
1. Is RabbitMQ running? `docker ps | grep rabbitmq`
2. Is port 5672 accessible? `telnet localhost 5672`
3. Are credentials correct in appsettings.json?

### Messages Not Being Consumed

**Check:**
1. Are consumers registered? Look for "Consumer started" in logs
2. Check RabbitMQ Management UI - are consumers connected?
3. Check application logs for errors

### High Memory Usage

**Reduce prefetch count in consumer:**
```csharp
protected override int PrefetchCount => 5; // Default is 10
```

## Next Steps

1. Read the full [Message Queue System Documentation](MESSAGE_QUEUE_SYSTEM.md)
2. Implement custom consumers for your business logic
3. Set up monitoring and alerting
4. Configure production settings
5. Implement message tracing and correlation

## Production Checklist

- [ ] Use strong RabbitMQ password
- [ ] Configure persistent storage for RabbitMQ
- [ ] Set up monitoring (Prometheus/Grafana)
- [ ] Configure health checks in orchestrator
- [ ] Set up alerting for queue depth
- [ ] Enable RabbitMQ clustering for HA
- [ ] Implement message encryption for sensitive data
- [ ] Configure backup strategy
- [ ] Set resource limits (CPU/Memory)
- [ ] Enable TLS for connections

## Additional Resources

- [Full Documentation](MESSAGE_QUEUE_SYSTEM.md)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/getstarted.html)
- [.NET RabbitMQ Client Guide](https://www.rabbitmq.com/dotnet-api-guide.html)
