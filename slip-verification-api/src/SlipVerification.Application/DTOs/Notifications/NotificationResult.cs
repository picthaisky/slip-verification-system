namespace SlipVerification.Application.DTOs.Notifications;

/// <summary>
/// Result of notification sending operation
/// </summary>
public class NotificationResult
{
    /// <summary>
    /// Gets or sets whether the notification was sent successfully
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Gets or sets the notification ID
    /// </summary>
    public Guid? NotificationId { get; set; }
    
    /// <summary>
    /// Gets or sets the error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the provider response ID
    /// </summary>
    public string? ProviderMessageId { get; set; }
    
    /// <summary>
    /// Gets or sets when the notification was sent
    /// </summary>
    public DateTime? SentAt { get; set; }
    
    /// <summary>
    /// Gets or sets additional response data
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
    
    /// <summary>
    /// Creates a success result
    /// </summary>
    public static NotificationResult CreateSuccess(Guid? notificationId = null, string? providerMessageId = null)
    {
        return new NotificationResult
        {
            Success = true,
            NotificationId = notificationId,
            ProviderMessageId = providerMessageId,
            SentAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a failure result
    /// </summary>
    public static NotificationResult CreateFailure(string errorMessage)
    {
        return new NotificationResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
