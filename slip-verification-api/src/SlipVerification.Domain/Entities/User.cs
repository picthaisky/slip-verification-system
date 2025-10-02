using SlipVerification.Domain.Common;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Domain.Entities;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the password hash
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the full name
    /// </summary>
    public string? FullName { get; set; }
    
    /// <summary>
    /// Gets or sets the phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the LINE Notify token for notifications
    /// </summary>
    public string? LineNotifyToken { get; set; }
    
    /// <summary>
    /// Gets or sets the user role
    /// </summary>
    public UserRole Role { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the email is verified
    /// </summary>
    public bool EmailVerified { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the last login date
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Gets or sets the number of failed login attempts
    /// </summary>
    public int FailedLoginAttempts { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the account is locked out
    /// </summary>
    public bool IsLockedOut { get; set; }
    
    /// <summary>
    /// Gets or sets the lockout end date
    /// </summary>
    public DateTime? LockoutEnd { get; set; }
    
    /// <summary>
    /// Gets or sets the email verification token
    /// </summary>
    public string? EmailVerificationToken { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is enabled
    /// </summary>
    public bool TwoFactorEnabled { get; set; }
    
    /// <summary>
    /// Navigation property for orders
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    
    /// <summary>
    /// Navigation property for refresh tokens
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
