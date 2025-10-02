# Security System Documentation

## Overview

This document describes the complete security system implemented for the Slip Verification System. The security implementation follows OWASP best practices and includes JWT authentication, role-based authorization, password security, audit logging, and security middleware.

## Architecture

The security system is built on the following layers:

### 1. Domain Layer
- **Entities**: User, RefreshToken, AuditLog
- **Enums**: UserRole (Guest, User, Manager, Admin)

### 2. Application Layer
- **Interfaces**: IJwtTokenService, IAuthenticationService, IPasswordHasher, IPasswordValidator, IAuditLogger, IUserPermissionService
- **DTOs**: LoginRequest, RegisterRequest, AuthenticationResult, RegistrationResult
- **Configuration**: JwtConfiguration, PasswordPolicyOptions, RateLimitOptions
- **Authorization**: Roles, Permissions, PermissionRequirement

### 3. Infrastructure Layer
- **Services**: JwtTokenService, AuthenticationService, BcryptPasswordHasher, PasswordValidator, AuditLogger, UserPermissionService
- **Repositories**: UserRepository, RefreshTokenRepository

### 4. API Layer
- **Controllers**: AuthController
- **Middleware**: SecurityHeadersMiddleware, RateLimitingMiddleware, InputSanitizationMiddleware
- **Authorization Handlers**: PermissionAuthorizationHandler

## Features

### 1. JWT Authentication

#### Access Tokens
- **Type**: JWT (JSON Web Tokens)
- **Algorithm**: HMAC-SHA256
- **Expiration**: Configurable (default: 60 minutes)
- **Claims**:
  - NameIdentifier (User ID)
  - Name (Username)
  - Email
  - Role(s)
  - Custom claims (e.g., FullName)

#### Refresh Tokens
- **Type**: Cryptographically secure random tokens
- **Storage**: Database with user association
- **Expiration**: Configurable (default: 7 days)
- **Features**:
  - Token rotation (old token revoked when refreshed)
  - IP address tracking
  - Active/inactive status
  - Revocation support

### 2. Authentication Service

#### Login Flow
```
1. User submits credentials (email + password)
2. System validates user exists
3. Check account lockout status
4. Verify password using BCrypt
5. Check email verification status
6. Reset failed login attempts on success
7. Generate access token and refresh token
8. Update last login timestamp
9. Return tokens and user info
```

#### Account Lockout
- **Failed Attempts Threshold**: 5 attempts
- **Lockout Duration**: 30 minutes
- **Auto-unlock**: After lockout period expires

#### Registration Flow
```
1. Validate email and username uniqueness
2. Validate password against policy
3. Hash password using BCrypt
4. Create user with default role (User)
5. Generate email verification token
6. Send verification email (TODO)
7. Return success response
```

### 3. Password Security

#### Password Hashing
- **Algorithm**: BCrypt
- **Work Factor**: 12 (2^12 iterations)
- **Salt**: Automatically generated per password

#### Password Policy
```json
{
  "MinimumLength": 8,
  "RequireUppercase": true,
  "RequireLowercase": true,
  "RequireDigit": true,
  "RequireSpecialCharacter": true
}
```

#### Password Validation Errors
- Password too short
- Missing uppercase letter
- Missing lowercase letter
- Missing digit
- Missing special character

### 4. Role-Based Access Control (RBAC)

#### Roles
| Role | Description | Level |
|------|-------------|-------|
| Guest | Limited read-only access | 0 |
| User | Standard user with basic permissions | 1 |
| Manager | Elevated permissions for management tasks | 2 |
| Admin | Full system access | 3 |

#### Permissions

**Slip Permissions:**
- `slips.view` - View payment slips
- `slips.upload` - Upload payment slips
- `slips.verify` - Verify payment slips
- `slips.delete` - Delete payment slips

**Order Permissions:**
- `orders.view` - View orders
- `orders.create` - Create new orders
- `orders.update` - Update existing orders
- `orders.delete` - Delete orders

**User Permissions:**
- `users.view` - View user information
- `users.manage` - Manage users (create, update, delete)

**Report Permissions:**
- `reports.view` - View reports
- `reports.export` - Export reports

#### Role-Permission Matrix

| Permission | Guest | User | Manager | Admin |
|------------|-------|------|---------|-------|
| slips.view | ✓ | ✓ | ✓ | ✓ |
| slips.upload | ✗ | ✓ | ✓ | ✓ |
| slips.verify | ✗ | ✗ | ✓ | ✓ |
| slips.delete | ✗ | ✗ | ✗ | ✓ |
| orders.view | ✓ | ✓ | ✓ | ✓ |
| orders.create | ✗ | ✓ | ✓ | ✓ |
| orders.update | ✗ | ✗ | ✓ | ✓ |
| orders.delete | ✗ | ✗ | ✗ | ✓ |
| users.view | ✗ | ✗ | ✓ | ✓ |
| users.manage | ✗ | ✗ | ✗ | ✓ |
| reports.view | ✗ | ✗ | ✓ | ✓ |
| reports.export | ✗ | ✗ | ✓ | ✓ |

### 5. Authorization Policies

#### Role-Based Policies
```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AdminOnly() { }

[Authorize(Policy = "ManagerOrAdmin")]
public async Task<IActionResult> ManagerOrAdmin() { }
```

#### Permission-Based Policies
```csharp
[Authorize(Policy = "CanVerifySlips")]
public async Task<IActionResult> VerifySlip() { }

[Authorize(Policy = "CanManageUsers")]
public async Task<IActionResult> ManageUsers() { }
```

### 6. Security Middleware

#### Security Headers Middleware
Adds security-related HTTP headers:
- `X-Frame-Options: DENY` - Prevents clickjacking
- `X-Content-Type-Options: nosniff` - Prevents MIME sniffing
- `X-XSS-Protection: 1; mode=block` - Enables XSS filter
- `Referrer-Policy: no-referrer` - Controls referrer information
- `Permissions-Policy` - Restricts browser features
- `Strict-Transport-Security` - Forces HTTPS
- `Content-Security-Policy` - Defines allowed content sources

#### Rate Limiting Middleware
Prevents abuse by limiting requests:
- **Max Requests**: 100 per time window (configurable)
- **Time Window**: 1 minute (configurable)
- **Tracking**: By IP address and endpoint
- **Response**: 429 Too Many Requests when exceeded

#### Input Sanitization Middleware
Protects against injection attacks:
- **SQL Injection Detection**: Scans for common SQL injection patterns
- **Request Types**: POST, PUT, PATCH with body content
- **Response**: 400 Bad Request when malicious input detected
- **Patterns Detected**:
  - `'; DROP TABLE`
  - `'; DELETE FROM`
  - `' OR '1'='1`
  - `' OR 1=1--`
  - SQL comments (`--`, `/*`, `*/`)

### 7. Audit Logging

#### Logged Information
- User ID (who performed the action)
- Action (CREATE, UPDATE, DELETE, etc.)
- Entity Type (table name)
- Entity ID
- Old Values (JSON)
- New Values (JSON)
- IP Address
- User Agent
- Timestamp

#### Usage
```csharp
await _auditLogger.LogAsync(
    action: "UPDATE",
    entityType: "User",
    entityId: userId,
    oldValues: oldUser,
    newValues: updatedUser
);
```

## API Endpoints

### Authentication Endpoints

#### POST /api/v1/auth/login
Login with email and password.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "gKFzQ3N5c3RlbS5Db2xsZWN0...",
  "user": {
    "id": "guid",
    "username": "johndoe",
    "email": "user@example.com",
    "fullName": "John Doe",
    "role": "User"
  }
}
```

#### POST /api/v1/auth/register
Register a new user.

**Request:**
```json
{
  "email": "newuser@example.com",
  "username": "newuser",
  "password": "SecurePassword123!",
  "fullName": "New User"
}
```

**Response:**
```json
{
  "message": "Registration successful. Please verify your email.",
  "user": {
    "id": "guid",
    "username": "newuser",
    "email": "newuser@example.com",
    "fullName": "New User",
    "role": "User"
  }
}
```

#### POST /api/v1/auth/refresh
Refresh access token using refresh token.

**Request:**
```json
{
  "refreshToken": "gKFzQ3N5c3RlbS5Db2xsZWN0..."
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "new_refresh_token...",
  "user": { ... }
}
```

#### POST /api/v1/auth/logout
Logout and revoke refresh tokens.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Response:**
```json
{
  "message": "Logged out successfully"
}
```

#### POST /api/v1/auth/change-password
Change user password.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request:**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewSecurePassword456!"
}
```

**Response:**
```json
{
  "message": "Password changed successfully"
}
```

#### GET /api/v1/auth/me
Get current user information.

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Response:**
```json
{
  "id": "guid",
  "username": "johndoe",
  "email": "user@example.com",
  "role": "User"
}
```

## Configuration

### appsettings.json

```json
{
  "Jwt": {
    "Secret": "YourSecretKeyHere_MustBeAtLeast32Characters",
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

## OWASP Top 10 Protection

### A01: Broken Access Control
- ✅ Role-Based Access Control (RBAC)
- ✅ Permission-based authorization
- ✅ JWT token validation
- ✅ Refresh token rotation

### A02: Cryptographic Failures
- ✅ BCrypt password hashing
- ✅ JWT token signing
- ✅ HTTPS enforcement (HSTS)
- ✅ Secure random token generation

### A03: Injection
- ✅ Input sanitization middleware
- ✅ Parameterized queries (Entity Framework)
- ✅ SQL injection pattern detection

### A04: Insecure Design
- ✅ Security by design principles
- ✅ Defense in depth
- ✅ Least privilege principle

### A05: Security Misconfiguration
- ✅ Security headers middleware
- ✅ CORS configuration
- ✅ Secure default settings

### A06: Vulnerable Components
- ✅ Regular dependency updates
- ✅ Package vulnerability scanning

### A07: Authentication Failures
- ✅ Strong password policy
- ✅ Account lockout mechanism
- ✅ Secure session management
- ✅ JWT with expiration

### A08: Software and Data Integrity Failures
- ✅ JWT digital signatures
- ✅ Token validation
- ✅ Audit logging

### A09: Security Logging and Monitoring Failures
- ✅ Comprehensive audit logging
- ✅ Structured logging (Serilog)
- ✅ Failed login tracking

### A10: Server-Side Request Forgery (SSRF)
- ✅ Input validation
- ✅ URL whitelisting (when applicable)

## Security Best Practices

### For Development
1. Never commit secrets to source control
2. Use environment variables for sensitive data
3. Keep dependencies up to date
4. Enable security warnings in IDE
5. Use code analysis tools

### For Production
1. Change default JWT secret
2. Use strong, unique secrets
3. Enable HTTPS only
4. Configure proper CORS origins
5. Set up monitoring and alerting
6. Regular security audits
7. Implement rate limiting
8. Enable audit logging
9. Regular backup of audit logs
10. Implement 2FA (future enhancement)

### For Users
1. Use strong, unique passwords
2. Don't share credentials
3. Logout when finished
4. Use secure networks
5. Keep browsers updated

## Database Schema

### Users Table
```sql
CREATE TABLE "Users" (
    "Id" uuid PRIMARY KEY,
    "Username" varchar(100) NOT NULL UNIQUE,
    "Email" varchar(255) NOT NULL UNIQUE,
    "PasswordHash" varchar(500) NOT NULL,
    "FullName" varchar(255),
    "PhoneNumber" varchar(20),
    "LineNotifyToken" varchar(255),
    "Role" integer NOT NULL,
    "EmailVerified" boolean NOT NULL DEFAULT false,
    "IsActive" boolean NOT NULL DEFAULT true,
    "LastLoginAt" timestamp,
    "FailedLoginAttempts" integer NOT NULL DEFAULT 0,
    "IsLockedOut" boolean NOT NULL DEFAULT false,
    "LockoutEnd" timestamp,
    "EmailVerificationToken" varchar(255),
    "TwoFactorEnabled" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp,
    "IsDeleted" boolean NOT NULL DEFAULT false
);
```

### RefreshTokens Table
```sql
CREATE TABLE "RefreshTokens" (
    "Id" uuid PRIMARY KEY,
    "Token" varchar(500) NOT NULL UNIQUE,
    "UserId" uuid NOT NULL,
    "ExpiresAt" timestamp NOT NULL,
    "IsActive" boolean NOT NULL,
    "RevokedAt" timestamp,
    "IpAddress" varchar(50),
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);
```

### AuditLogs Table
```sql
CREATE TABLE "AuditLogs" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid,
    "EntityType" varchar NOT NULL,
    "EntityId" uuid NOT NULL,
    "Action" varchar NOT NULL,
    "OldValues" text,
    "NewValues" text,
    "IpAddress" varchar,
    "UserAgent" varchar,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);
```

## Future Enhancements

### Planned Features
1. **Two-Factor Authentication (2FA)**
   - TOTP (Time-based One-Time Password)
   - SMS verification
   - Email verification codes

2. **OAuth 2.0 / OpenID Connect**
   - Google login
   - Facebook login
   - Microsoft login

3. **Advanced Security**
   - Device fingerprinting
   - Anomaly detection
   - Geographic restrictions
   - Session management dashboard

4. **Compliance**
   - GDPR compliance tools
   - Data export/deletion
   - Privacy policy enforcement

## Troubleshooting

### Common Issues

#### Invalid Token
- **Cause**: Expired token or invalid signature
- **Solution**: Refresh the token or login again

#### Account Locked
- **Cause**: Multiple failed login attempts
- **Solution**: Wait 30 minutes or contact admin

#### Password Requirements Not Met
- **Cause**: Password doesn't meet policy
- **Solution**: Follow password policy requirements

#### Rate Limit Exceeded
- **Cause**: Too many requests in short time
- **Solution**: Wait and retry after time window

## Support

For security issues or questions:
- Email: security@slipverification.com
- Report vulnerabilities: security-reports@slipverification.com

---

Last Updated: 2024
Version: 1.0
