using SlipVerification.Domain.Enums;

namespace SlipVerification.Application.DTOs.Notifications;

/// <summary>
/// Request DTO for sending a notification
/// </summary>
public class SendNotificationRequest
{
    /// <summary>
    /// Gets or sets the user ID to send notification to
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the notification channel
    /// </summary>
    public NotificationChannel Channel { get; set; }
    
    /// <summary>
    /// Gets or sets the notification priority
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    
    /// <summary>
    /// Gets or sets the notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the notification message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the template code (optional)
    /// </summary>
    public string? TemplateCode { get; set; }
    
    /// <summary>
    /// Gets or sets template placeholders
    /// </summary>
    public Dictionary<string, string>? Placeholders { get; set; }
    
    /// <summary>
    /// Gets or sets additional data
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}
