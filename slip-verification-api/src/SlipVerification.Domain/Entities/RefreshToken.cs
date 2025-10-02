using SlipVerification.Domain.Common;

namespace SlipVerification.Domain.Entities;

/// <summary>
/// Represents a refresh token for authentication
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// Gets or sets the token value
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration date
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the token is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets the revoked date
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the IP address where token was created
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Navigation property for user
    /// </summary>
    public virtual User User { get; set; } = null!;
}
