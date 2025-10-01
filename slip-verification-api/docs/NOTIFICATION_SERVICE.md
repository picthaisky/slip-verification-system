# Notification Service Documentation

## Overview

The Notification Service is a comprehensive multi-channel notification system that supports LINE Notify, Email (SMTP), Push Notifications (FCM), and SMS (Twilio). It features template-based messaging, queue processing, retry mechanisms, rate limiting, and webhook callbacks.

## Architecture

```
┌─────────────────┐
│  API Controller │
└────────┬────────┘
         │
         ▼
┌─────────────────────────┐      ┌──────────────────┐
│  Notification Service   │◄─────┤  Template Engine │
└────────┬────────────────┘      └──────────────────┘
         │
         ▼
┌─────────────────────────┐      ┌──────────────────┐
│   Rate Limiter (Redis)  │◄─────┤  Channel Router  │
└─────────────────────────┘      └────────┬─────────┘
                                           │
         ┌─────────────────────────────────┼─────────────────────────┐
         │                                 │                         │
         ▼                                 ▼                         ▼
┌─────────────────┐            ┌──────────────────┐      ┌──────────────────┐
│  LINE Channel   │            │  Email Channel   │      │  Push Channel    │
└─────────────────┘            └──────────────────┘      └──────────────────┘
         │                                 │                         │
         ▼                                 ▼                         ▼
┌─────────────────┐            ┌──────────────────┐      ┌──────────────────┐
│  LINE Notify    │            │     SMTP         │      │       FCM        │
└─────────────────┘            └──────────────────┘      └──────────────────┘

         ┌─────────────────────────────────┐
         │  Notification Queue Processor   │
         │   (Background Service)          │
         └─────────────────────────────────┘
```

## Features

### 1. Multi-Channel Support

- **LINE Notify**: Send messages with images to LINE
- **Email**: HTML emails via SMTP with attachment support
- **Push Notifications**: FCM-based push notifications for mobile apps
- **SMS**: Text messages via Twilio

### 2. Template Engine

Templates support placeholders in `{{placeholderName}}` format.

**Example Template:**
```
Subject: Payment Received - Order {{orderNumber}}
Body: Your payment of {{amount}} THB has been received for order {{orderNumber}}.
```

**Available Templates:**
- `payment_received` - Payment confirmation
- `payment_verified` - Payment verification success
- `payment_failed` - Payment verification failure
- `order_created` - New order notification
- `order_cancelled` - Order cancellation

### 3. Rate Limiting

Per-channel rate limits (configurable via Redis):
- **LINE Notify**: 1000 calls/hour
- **Email**: 100 emails/minute
- **SMS**: 10 SMS/minute
- **Push**: 500 notifications/minute

### 4. Retry Mechanism

Automatic retry with exponential backoff:
- Max retries: 3 (configurable)
- Backoff: 2^retryCount seconds
- Circuit breaker pattern via Polly

### 5. Queue Processing

Background service for asynchronous notification processing:
- In-memory queue with System.Threading.Channels
- Concurrent processing (10 workers by default)
- Automatic retry for failed notifications

## API Endpoints

### Send Notification Immediately

```http
POST /api/v1/notifications/send
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "channel": 0,  // 0=LINE, 1=EMAIL, 2=PUSH, 3=SMS
  "priority": 1,  // 0=Low, 1=Normal, 2=High, 3=Urgent
  "title": "Test Notification",
  "message": "This is a test message",
  "templateCode": "payment_received",
  "placeholders": {
    "orderNumber": "ORD-12345",
    "amount": "1500.00"
  }
}
```

### Queue Notification for Async Processing

```http
POST /api/v1/notifications/queue
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "channel": 1,
  "priority": 2,
  "title": "Important Email",
  "message": "This will be sent asynchronously"
}
```

### Get Notification by ID

```http
GET /api/v1/notifications/{id}
Authorization: Bearer {token}
```

### Get My Notifications

```http
GET /api/v1/notifications/my?page=1&pageSize=20
Authorization: Bearer {token}
```

### Mark as Read

```http
PUT /api/v1/notifications/{id}/read
Authorization: Bearer {token}
```

### Retry Failed Notification

```http
POST /api/v1/notifications/{id}/retry
Authorization: Bearer {token}
```

## Configuration

### appsettings.json

```json
{
  "LineNotify": {
    "DefaultToken": "your-line-notify-token",
    "TimeoutSeconds": 30
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@slipverification.com",
    "FromName": "Slip Verification System",
    "EnableSsl": true
  },
  "PushNotification": {
    "ProjectId": "your-firebase-project-id",
    "ServerKey": "your-firebase-server-key",
    "TimeoutSeconds": 30
  },
  "Sms": {
    "AccountSid": "your-twilio-account-sid",
    "AuthToken": "your-twilio-auth-token",
    "FromNumber": "+1234567890",
    "TimeoutSeconds": 30
  }
}
```

## Usage Examples

### 1. Send LINE Notification with Template

```csharp
var notificationService = serviceProvider.GetRequiredService<INotificationService>();

var message = new NotificationMessage
{
    UserId = userId,
    Channel = NotificationChannel.LINE,
    Priority = NotificationPriority.Normal,
    TemplateCode = "payment_received",
    Placeholders = new Dictionary<string, string>
    {
        { "orderNumber", "ORD-12345" },
        { "amount", "1500.00" },
        { "paymentDate", DateTime.Now.ToString("dd/MM/yyyy") }
    },
    LineToken = "your-user-line-token"
};

var result = await notificationService.SendNotificationAsync(message);
if (result.Success)
{
    Console.WriteLine($"Notification sent! ID: {result.NotificationId}");
}
```

### 2. Queue Notification for Background Processing

```csharp
var queueService = serviceProvider.GetRequiredService<INotificationQueueService>();

var message = new NotificationMessage
{
    UserId = userId,
    Channel = NotificationChannel.EMAIL,
    Priority = NotificationPriority.High,
    Title = "Payment Verified",
    Message = "Your payment has been verified successfully",
    RecipientEmail = "user@example.com"
};

await queueService.EnqueueAsync(message);
```

### 3. Send Email with Custom HTML

```csharp
var message = new NotificationMessage
{
    UserId = userId,
    Channel = NotificationChannel.EMAIL,
    Title = "Order Confirmation",
    Message = "<h1>Thank you!</h1><p>Your order has been confirmed.</p>",
    RecipientEmail = "customer@example.com"
};

await notificationService.SendNotificationAsync(message);
```

## Database Schema

### Notifications Table

```sql
CREATE TABLE "Notifications" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Type" VARCHAR(50) NOT NULL,
    "Title" VARCHAR(255) NOT NULL,
    "Message" TEXT NOT NULL,
    "Data" JSONB,
    "Channel" VARCHAR(50) NOT NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    "Priority" INTEGER NOT NULL,
    "SentAt" TIMESTAMP,
    "ReadAt" TIMESTAMP,
    "ErrorMessage" TEXT,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "MaxRetryCount" INTEGER NOT NULL DEFAULT 3,
    "NextRetryAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);
```

### NotificationTemplates Table

```sql
CREATE TABLE "NotificationTemplates" (
    "Id" UUID PRIMARY KEY,
    "Code" VARCHAR(100) NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "Channel" INTEGER NOT NULL,
    "Subject" VARCHAR(500) NOT NULL,
    "Body" TEXT NOT NULL,
    "Language" VARCHAR(10) NOT NULL DEFAULT 'en',
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "Metadata" JSONB,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    UNIQUE ("Code", "Channel", "Language")
);
```

## Deployment

### 1. Update Database

```bash
cd slip-verification-api/src/SlipVerification.Infrastructure
dotnet ef database update --startup-project ../SlipVerification.API
```

### 2. Seed Templates

```bash
psql -U postgres -d SlipVerificationDb -f database/scripts/seed/notification_templates.sql
```

### 3. Configure Environment Variables

```bash
export Email__Username="your-email@gmail.com"
export Email__Password="your-app-password"
export LineNotify__DefaultToken="your-line-token"
export PushNotification__ServerKey="your-fcm-key"
export Sms__AccountSid="your-twilio-sid"
export Sms__AuthToken="your-twilio-token"
```

### 4. Docker Compose

The notification service requires Redis for rate limiting:

```yaml
services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
```

## Monitoring

### Health Checks

```http
GET /health
```

Response includes notification service status.

### Logs

Notifications are logged with Serilog:
- INFO: Successful sends
- WARNING: Rate limits, retries
- ERROR: Failed sends, exceptions

### Metrics to Monitor

- Notification send rate per channel
- Failed notification rate
- Average retry count
- Queue depth
- Rate limit hits

## Troubleshooting

### LINE Notify Issues

**Problem**: "Invalid token" error
- Verify LINE token is valid
- Check token permissions
- Ensure token hasn't expired

### Email Issues

**Problem**: SMTP authentication failed
- Use app-specific password for Gmail
- Enable "Less secure app access" or use OAuth2
- Check SMTP host and port

### FCM Issues

**Problem**: Push notifications not delivered
- Verify server key is correct
- Check device token is valid
- Ensure FCM project ID matches

### SMS Issues

**Problem**: SMS not sent
- Verify Twilio Account SID and Auth Token
- Check phone number format (E.164)
- Ensure sufficient Twilio credits

## Best Practices

1. **Use Templates**: Define reusable templates for common notifications
2. **Queue Long Operations**: Use queue for bulk notifications
3. **Monitor Rate Limits**: Watch for rate limit hits
4. **Set Appropriate Priority**: Reserve High/Urgent for critical notifications
5. **Handle Failures**: Implement webhook callbacks for failure handling
6. **Test Thoroughly**: Test each channel in staging before production

## Security Considerations

1. **Credentials**: Store sensitive credentials in environment variables or Azure Key Vault
2. **Rate Limiting**: Prevents abuse and API overload
3. **Authentication**: All endpoints require JWT authentication
4. **Data Privacy**: Notification data includes PII - handle accordingly
5. **Webhooks**: Validate webhook callback URLs before calling

## Future Enhancements

- RabbitMQ/Azure Service Bus integration for distributed queue
- Notification preferences per user
- Delivery status tracking
- A/B testing for notification content
- Analytics dashboard
- Multi-language support expansion

## Support

For issues or questions:
- GitHub Issues: [Repository Issues](https://github.com/picthaisky/slip-verification-system/issues)
- Email: support@slipverification.com
