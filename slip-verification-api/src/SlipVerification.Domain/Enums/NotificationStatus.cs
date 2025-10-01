namespace SlipVerification.Domain.Enums;

/// <summary>
/// Represents the status of a notification
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Notification is pending to be sent
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Notification is being processed
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Notification was sent successfully
    /// </summary>
    Sent = 2,
    
    /// <summary>
    /// Notification sending failed
    /// </summary>
    Failed = 3,
    
    /// <summary>
    /// Notification is being retried
    /// </summary>
    Retrying = 4,
    
    /// <summary>
    /// Notification was cancelled
    /// </summary>
    Cancelled = 5
}
