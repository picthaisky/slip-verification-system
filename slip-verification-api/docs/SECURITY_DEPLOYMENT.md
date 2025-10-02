# Security System Deployment Guide

## Overview

This guide covers the deployment of the security system enhancements to the Slip Verification API.

## Prerequisites

- PostgreSQL 16+ database
- .NET 9.0 SDK
- Access to the database server

## Database Migration

### Option 1: Using EF Core Migrations (Recommended)

```bash
cd slip-verification-api/src/SlipVerification.Infrastructure
dotnet ef database update --startup-project ../SlipVerification.API
```

This will apply the `AddSecurityEnhancements` migration which:
- Adds security fields to Users table
- Creates RefreshTokens table
- Adds necessary indexes

### Option 2: Manual SQL Script

If you prefer to review and run the SQL manually:

```bash
cd slip-verification-api/src/SlipVerification.Infrastructure
dotnet ef migrations script --startup-project ../SlipVerification.API --output security-migration.sql
```

Then review and execute the generated SQL script.

## Configuration Updates

### 1. Update appsettings.json

Add the following sections to your `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "CHANGE_THIS_TO_A_SECURE_SECRET_AT_LEAST_32_CHARACTERS_LONG",
    "Issuer": "SlipVerificationAPI",
    "Audience": "SlipVerificationClient",
    "ExpiryMinutes": "60",
    "RefreshTokenExpirationDays": "7",
    "ClockSkewMinutes": "5"
  },
  "PasswordPolicy": {
    "MinimumLength": 8,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireDigit": true,
    "RequireSpecialCharacter": true
  },
  "RateLimit": {
    "MaxRequests": 100,
    "TimeWindow": "00:01:00"
  }
}
```

### 2. Environment Variables (Production)

For production, use environment variables instead of appsettings.json:

```bash
export Jwt__Secret="your-secure-secret-key-here"
export PasswordPolicy__MinimumLength="10"
export RateLimit__MaxRequests="50"
```

## Verification

### 1. Build the Application

```bash
cd slip-verification-api
dotnet build
```

Expected output: `Build succeeded`

### 2. Run Unit Tests

```bash
cd slip-verification-api
dotnet test --filter "FullyQualifiedName!~IntegrationTests"
```

### 3. Test Authentication Endpoints

#### Register a new user
```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "testuser",
    "password": "SecurePass123!",
    "fullName": "Test User"
  }'
```

#### Login
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePass123!"
  }'
```

Save the `accessToken` and `refreshToken` from the response.

#### Get current user info
```bash
curl -X GET http://localhost:5000/api/v1/auth/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

#### Refresh token
```bash
curl -X POST http://localhost:5000/api/v1/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

## Security Checklist

Before deploying to production:

- [ ] Change the JWT secret to a strong, unique value
- [ ] Enable HTTPS and configure SSL certificates
- [ ] Set up proper CORS origins (remove wildcard)
- [ ] Configure environment-specific settings
- [ ] Set up monitoring and alerting
- [ ] Review and adjust rate limiting settings
- [ ] Enable audit log rotation
- [ ] Set up backup for audit logs
- [ ] Test account lockout mechanism
- [ ] Test password policy enforcement
- [ ] Review and test authorization policies
- [ ] Set up database backups
- [ ] Configure log aggregation (ELK, etc.)

## Post-Deployment

### 1. Create Admin User

After deployment, create an initial admin user:

```sql
-- Connect to your database
psql -U postgres -d SlipVerificationDb

-- Insert admin user (password: Admin123!)
INSERT INTO "Users" (
    "Id", "Username", "Email", "PasswordHash", 
    "Role", "EmailVerified", "IsActive", "CreatedAt"
) VALUES (
    gen_random_uuid(),
    'admin',
    'admin@slipverification.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5iSy6rXm.3Zfi',
    3,
    true,
    true,
    NOW()
);
```

Note: This is a sample password. Change it immediately after first login.

### 2. Monitor Logs

Monitor application logs for:
- Failed login attempts
- Account lockouts
- Invalid token usage
- Rate limit violations
- SQL injection attempts

### 3. Review Audit Logs

Periodically review audit logs:

```sql
SELECT * FROM "AuditLogs" 
ORDER BY "CreatedAt" DESC 
LIMIT 100;
```

## Rollback Procedure

If you need to rollback the security enhancements:

```bash
cd slip-verification-api/src/SlipVerification.Infrastructure
dotnet ef database update AddNotificationEnhancements --startup-project ../SlipVerification.API
```

This will revert to the previous migration.

## Troubleshooting

### Migration Fails

**Error**: "Cannot connect to database"
**Solution**: Check database connection string and ensure PostgreSQL is running

**Error**: "Column already exists"
**Solution**: Database may already have the changes. Check with:
```sql
\d "Users"
\d "RefreshTokens"
```

### Authentication Issues

**Error**: "Invalid JWT secret not configured"
**Solution**: Ensure JWT:Secret is set in configuration

**Error**: "Token validation failed"
**Solution**: Check token expiration and clock skew settings

### Performance Issues

**Issue**: Slow login/authentication
**Solution**: 
- Ensure database indexes are created
- Check BCrypt work factor (default: 12)
- Review rate limiting settings

### Rate Limiting Issues

**Issue**: Legitimate users getting 429 errors
**Solution**: Adjust `RateLimit:MaxRequests` and `TimeWindow` settings

## Support

For issues or questions:
- Review SECURITY.md documentation
- Check application logs
- Review audit logs
- Contact: support@slipverification.com

## Next Steps

After successful deployment:

1. Set up monitoring dashboards
2. Configure alerting rules
3. Schedule regular security audits
4. Plan for 2FA implementation
5. Consider OAuth integration
6. Set up automated backups
7. Document incident response procedures

---

Last Updated: 2024
Version: 1.0
