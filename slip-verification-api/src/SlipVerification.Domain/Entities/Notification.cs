using SlipVerification.Domain.Common;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Domain.Entities;

/// <summary>
/// Represents a notification sent to users
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// Gets or sets the user ID to send notification to
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the notification type
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the notification message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets additional notification data (JSON)
    /// </summary>
    public string? Data { get; set; }
    
    /// <summary>
    /// Gets or sets the notification channel (LINE, EMAIL, PUSH, SMS)
    /// </summary>
    public string Channel { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the notification status
    /// </summary>
    public string Status { get; set; } = "Pending";
    
    /// <summary>
    /// Gets or sets the notification priority
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    
    /// <summary>
    /// Gets or sets when the notification was sent
    /// </summary>
    public DateTime? SentAt { get; set; }
    
    /// <summary>
    /// Gets or sets when the notification was read
    /// </summary>
    public DateTime? ReadAt { get; set; }
    
    /// <summary>
    /// Gets or sets the error message if sending failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the number of retry attempts
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum retry attempts
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;
    
    /// <summary>
    /// Gets or sets the next retry time
    /// </summary>
    public DateTime? NextRetryAt { get; set; }
    
    /// <summary>
    /// Navigation property for user
    /// </summary>
    public virtual User User { get; set; } = null!;
}
