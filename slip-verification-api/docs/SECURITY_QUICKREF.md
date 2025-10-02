# Security System Quick Reference

## API Endpoints

### Authentication

#### Register New User
```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "username": "username",
  "password": "SecurePass123!",
  "fullName": "Full Name"
}
```

#### Login
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}

Response: {
  "accessToken": "eyJhbG...",
  "refreshToken": "base64token...",
  "user": { ... }
}
```

#### Refresh Token
```http
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "your-refresh-token"
}
```

#### Logout
```http
POST /api/v1/auth/logout
Authorization: Bearer {accessToken}
```

#### Change Password
```http
POST /api/v1/auth/change-password
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass123!"
}
```

#### Get Current User
```http
GET /api/v1/auth/me
Authorization: Bearer {accessToken}
```

## Authorization in Controllers

### Role-Based
```csharp
using SlipVerification.Application.Authorization;

[Authorize(Roles = Roles.Admin)]
public async Task<IActionResult> AdminOnly() { }

[Authorize(Roles = $"{Roles.Manager},{Roles.Admin}")]
public async Task<IActionResult> ManagerOrAdmin() { }
```

### Permission-Based
```csharp
[Authorize(Policy = "CanVerifySlips")]
public async Task<IActionResult> VerifySlip() { }

[Authorize(Policy = "CanManageUsers")]
public async Task<IActionResult> ManageUsers() { }
```

### Available Policies
- `AdminOnly` - Admin role only
- `ManagerOrAdmin` - Manager or Admin roles
- `CanViewSlips` - View slips permission
- `CanUploadSlips` - Upload slips permission
- `CanVerifySlips` - Verify slips permission
- `CanDeleteSlips` - Delete slips permission
- `CanManageUsers` - Manage users permission

## Using Services

### Authentication Service
```csharp
public class MyController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    
    public MyController(IAuthenticationService authService)
    {
        _authService = authService;
    }
    
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success)
            return Unauthorized(result.Message);
        return Ok(result);
    }
}
```

### Password Validation
```csharp
public class MyService
{
    private readonly IPasswordValidator _validator;
    
    public MyService(IPasswordValidator validator)
    {
        _validator = validator;
    }
    
    public bool ValidatePassword(string password)
    {
        var result = _validator.Validate(password);
        if (!result.IsValid)
        {
            // Handle errors
            foreach (var error in result.Errors)
            {
                // Log or return error
            }
            return false;
        }
        return true;
    }
}
```

### Audit Logging
```csharp
public class MyService
{
    private readonly IAuditLogger _auditLogger;
    
    public MyService(IAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }
    
    public async Task UpdateEntity(MyEntity entity)
    {
        var oldValues = entity; // Get before update
        
        // Perform update
        await _repository.UpdateAsync(entity);
        
        // Log the change
        await _auditLogger.LogAsync(
            action: "UPDATE",
            entityType: "MyEntity",
            entityId: entity.Id,
            oldValues: oldValues,
            newValues: entity
        );
    }
}
```

### Get Current User Info
```csharp
[Authorize]
public class MyController : ControllerBase
{
    public IActionResult GetUserInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        
        return Ok(new { userId, username, email, role });
    }
}
```

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-chars",
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

### Environment Variables
```bash
Jwt__Secret=your-production-secret
PasswordPolicy__MinimumLength=10
RateLimit__MaxRequests=50
```

## Roles and Permissions

### Roles
- **Guest** (0) - Limited read access
- **User** (1) - Standard user
- **Manager** (2) - Management access
- **Admin** (3) - Full access

### Permission Constants
```csharp
using SlipVerification.Application.Authorization;

// Slip permissions
Permissions.ViewSlips
Permissions.UploadSlips
Permissions.VerifySlips
Permissions.DeleteSlips

// Order permissions
Permissions.ViewOrders
Permissions.CreateOrders
Permissions.UpdateOrders
Permissions.DeleteOrders

// User permissions
Permissions.ViewUsers
Permissions.ManageUsers

// Report permissions
Permissions.ViewReports
Permissions.ExportReports
```

## Common Scenarios

### Registering and Logging In
```bash
# Register
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","username":"test","password":"Test123!","fullName":"Test User"}'

# Login
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!"}'
```

### Using Access Token
```bash
# Get current user
curl -X GET http://localhost:5000/api/v1/auth/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"

# Access protected endpoint
curl -X GET http://localhost:5000/api/v1/slips \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### Refreshing Token
```bash
curl -X POST http://localhost:5000/api/v1/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"YOUR_REFRESH_TOKEN"}'
```

## Error Responses

### 400 Bad Request
```json
{
  "errors": [
    "Password must be at least 8 characters",
    "Password must contain at least one uppercase letter"
  ]
}
```

### 401 Unauthorized
```json
{
  "error": "Invalid email or password"
}
```

### 403 Forbidden
```json
{
  "error": "You do not have permission to perform this action"
}
```

### 429 Too Many Requests
```json
{
  "error": "Too many requests. Please try again later.",
  "retryAfter": 60
}
```

## Testing

### Unit Tests
```bash
dotnet test --filter "FullyQualifiedName!~IntegrationTests"
```

### Integration Tests (with database)
```bash
dotnet test tests/SlipVerification.IntegrationTests
```

## Troubleshooting

### Token Invalid
- Check token expiration
- Verify JWT secret matches
- Check issuer and audience

### Account Locked
- Wait 30 minutes or contact admin
- Check `FailedLoginAttempts` in database

### Password Requirements Not Met
- Must be 8+ characters
- Must have uppercase letter
- Must have lowercase letter
- Must have digit
- Must have special character

### Rate Limited
- Wait for time window to pass (default 1 minute)
- Reduce request frequency

## Security Best Practices

1. **Never log tokens** - Don't log access or refresh tokens
2. **Use HTTPS** - Always use HTTPS in production
3. **Secure secrets** - Store JWT secret securely
4. **Regular rotation** - Rotate secrets periodically
5. **Monitor logs** - Watch for suspicious activity
6. **Update dependencies** - Keep packages up to date
7. **Validate input** - Always validate user input
8. **Use prepared statements** - EF Core handles this
9. **Limit permissions** - Follow least privilege
10. **Audit everything** - Log important actions

## Quick Links

- [Full Documentation](SECURITY.md)
- [Deployment Guide](SECURITY_DEPLOYMENT.md)
- [API Reference](../swagger)

---

Last Updated: 2024
