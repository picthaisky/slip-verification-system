using SlipVerification.Domain.Common;

namespace SlipVerification.Domain.Entities;

/// <summary>
/// Represents an audit log entry tracking entity changes
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the user ID who performed the action
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the entity type (table name)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the entity ID
    /// </summary>
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the action performed (CREATE, UPDATE, DELETE)
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the old values (JSON)
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// Gets or sets the new values (JSON)
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// Gets or sets the IP address of the user
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the user agent string
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Navigation property for user
    /// </summary>
    public virtual User? User { get; set; }
}
