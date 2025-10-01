# Notification Service Quick Start Guide

## Prerequisites

- .NET 9 SDK
- PostgreSQL 15+
- Redis 7+
- (Optional) LINE Notify account
- (Optional) SMTP credentials
- (Optional) Firebase Cloud Messaging setup
- (Optional) Twilio account

## 1. Database Setup

### Apply Migrations

```bash
cd slip-verification-api/src/SlipVerification.Infrastructure
dotnet ef database update --startup-project ../SlipVerification.API
```

### Seed Notification Templates

```bash
psql -U postgres -d SlipVerificationDb -f ../../database/scripts/seed/notification_templates.sql
```

Or using pgAdmin:
1. Connect to SlipVerificationDb
2. Open Query Tool
3. Execute the content of `notification_templates.sql`

## 2. Configuration

### Option A: Environment Variables (Recommended for Production)

```bash
export ConnectionStrings__Redis="localhost:6379"
export LineNotify__DefaultToken="YOUR_LINE_TOKEN"
export Email__Username="your-email@gmail.com"
export Email__Password="your-app-password"
export PushNotification__ProjectId="your-firebase-project"
export PushNotification__ServerKey="your-fcm-key"
export Sms__AccountSid="your-twilio-sid"
export Sms__AuthToken="your-twilio-token"
export Sms__FromNumber="+1234567890"
```

### Option B: appsettings.Development.json (For Local Development)

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SlipVerificationDb;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "LineNotify": {
    "DefaultToken": "YOUR_LINE_TOKEN",
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
  }
}
```

## 3. Start Services

### Start Redis

```bash
docker run -d -p 6379:6379 redis:7-alpine
```

### Start the API

```bash
cd slip-verification-api/src/SlipVerification.API
dotnet run
```

API will be available at `http://localhost:5000` or `https://localhost:5001`

## 4. Test the API

### Using Swagger

1. Navigate to `http://localhost:5000/swagger`
2. Authenticate using JWT token
3. Try the `/api/v1/notifications/send` endpoint

### Using cURL

```bash
# Get your JWT token first (login)
TOKEN="your-jwt-token"

# Send a LINE notification
curl -X POST "http://localhost:5000/api/v1/notifications/send" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "channel": 0,
    "priority": 1,
    "title": "Test Notification",
    "message": "This is a test from the Notification Service!"
  }'
```

### Using Postman

Import the following collection:

```json
{
  "info": {
    "name": "Notification Service",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Send Notification",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"userId\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\n  \"channel\": 0,\n  \"priority\": 1,\n  \"title\": \"Test\",\n  \"message\": \"Test message\"\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": {
          "raw": "http://localhost:5000/api/v1/notifications/send",
          "protocol": "http",
          "host": ["localhost"],
          "port": "5000",
          "path": ["api", "v1", "notifications", "send"]
        }
      }
    }
  ]
}
```

## 5. Test with Templates

### Using Existing Templates

```bash
curl -X POST "http://localhost:5000/api/v1/notifications/send" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "channel": 1,
    "priority": 1,
    "templateCode": "payment_received",
    "placeholders": {
      "orderNumber": "ORD-12345",
      "amount": "1500.00",
      "paymentDate": "2024-10-01",
      "transactionId": "TXN-98765"
    }
  }'
```

## 6. Common Issues & Solutions

### Issue: "Redis connection failed"

**Solution:**
- Ensure Redis is running: `docker ps | grep redis`
- Check connection string in appsettings.json
- Test connection: `redis-cli ping` (should return PONG)

### Issue: "LINE API returns 401 Unauthorized"

**Solution:**
- Verify your LINE Notify token is correct
- Generate a new token at: https://notify-bot.line.me/my/
- Update the token in configuration

### Issue: "Email not sending"

**Solution:**
For Gmail:
1. Enable 2-factor authentication
2. Generate an App Password: https://myaccount.google.com/apppasswords
3. Use the app password instead of your regular password

### Issue: "Rate limit exceeded"

**Solution:**
- Check Redis for rate limit keys: `redis-cli KEYS "ratelimit:*"`
- Wait for the rate limit window to reset
- Adjust rate limits in `RateLimiter.cs` if needed

### Issue: "Database migration failed"

**Solution:**
```bash
# Reset migrations
dotnet ef database drop --startup-project ../SlipVerification.API
dotnet ef database update --startup-project ../SlipVerification.API
```

## 7. Docker Deployment

### Using Docker Compose

```bash
# Copy environment template
cp .env.notification.example .env

# Edit .env and add your credentials
nano .env

# Start all services
docker-compose -f docker-compose.frontend.yml up -d

# Check logs
docker logs slip-verification-backend -f
```

### Verify Services

```bash
# Check all containers are running
docker ps

# Test API health
curl http://localhost:5000/health

# Test notification endpoint
curl http://localhost:5000/api/v1/notifications/send \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"userId":"...", "channel":0, "message":"Test"}'
```

## 8. Monitoring

### Check Notification Status

```bash
# Get notification by ID
curl http://localhost:5000/api/v1/notifications/{notification-id} \
  -H "Authorization: Bearer $TOKEN"

# Get my notifications
curl http://localhost:5000/api/v1/notifications/my?page=1&pageSize=20 \
  -H "Authorization: Bearer $TOKEN"
```

### Check Logs

```bash
# Application logs
tail -f slip-verification-api/src/SlipVerification.API/Logs/log-*.txt

# Docker logs
docker logs slip-verification-backend -f --tail 100
```

### Monitor Redis

```bash
redis-cli
> KEYS ratelimit:*
> GET ratelimit:line:user:3fa85f64-5717-4562-b3fc-2c963f66afa6
> TTL ratelimit:line:user:3fa85f64-5717-4562-b3fc-2c963f66afa6
```

## 9. Next Steps

- Read full documentation: [NOTIFICATION_SERVICE.md](./NOTIFICATION_SERVICE.md)
- Configure additional channels (Push, SMS)
- Create custom notification templates
- Set up monitoring and alerts
- Configure production credentials
- Implement unit and integration tests

## Need Help?

- Documentation: `/docs/NOTIFICATION_SERVICE.md`
- API Docs: `http://localhost:5000/swagger`
- Issues: https://github.com/picthaisky/slip-verification-system/issues
