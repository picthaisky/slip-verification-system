using SlipVerification.Domain.Enums;

namespace SlipVerification.Application.DTOs.Notifications;

/// <summary>
/// DTO for notification message
/// </summary>
public class NotificationMessage
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
    /// Gets or sets additional notification data
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
    
    /// <summary>
    /// Gets or sets the template code (if using template)
    /// </summary>
    public string? TemplateCode { get; set; }
    
    /// <summary>
    /// Gets or sets template placeholders
    /// </summary>
    public Dictionary<string, string>? Placeholders { get; set; }
    
    /// <summary>
    /// Gets or sets the language code
    /// </summary>
    public string Language { get; set; } = "en";
    
    /// <summary>
    /// Gets or sets the recipient email (for email channel)
    /// </summary>
    public string? RecipientEmail { get; set; }
    
    /// <summary>
    /// Gets or sets the recipient phone (for SMS channel)
    /// </summary>
    public string? RecipientPhone { get; set; }
    
    /// <summary>
    /// Gets or sets the device token (for push notifications)
    /// </summary>
    public string? DeviceToken { get; set; }
    
    /// <summary>
    /// Gets or sets the LINE token (for LINE Notify)
    /// </summary>
    public string? LineToken { get; set; }
    
    /// <summary>
    /// Gets or sets the image URL/path to attach
    /// </summary>
    public string? ImagePath { get; set; }
    
    /// <summary>
    /// Gets or sets the callback URL for webhook
    /// </summary>
    public string? CallbackUrl { get; set; }
}
