using SlipVerification.Domain.Common;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Domain.Entities;

/// <summary>
/// Represents a notification template for different channels and types
/// </summary>
public class NotificationTemplate : BaseEntity
{
    /// <summary>
    /// Gets or sets the template code (unique identifier)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the template name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the notification channel
    /// </summary>
    public NotificationChannel Channel { get; set; }
    
    /// <summary>
    /// Gets or sets the subject/title template (for email/push)
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the body template with placeholders
    /// </summary>
    public string Body { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the language code (e.g., "th", "en")
    /// </summary>
    public string Language { get; set; } = "en";
    
    /// <summary>
    /// Gets or sets whether this template is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Gets or sets additional template metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }
}
