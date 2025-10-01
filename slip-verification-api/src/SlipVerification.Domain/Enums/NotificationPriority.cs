namespace SlipVerification.Domain.Enums;

/// <summary>
/// Represents the priority level of a notification
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// Low priority - can be delayed
    /// </summary>
    Low = 0,
    
    /// <summary>
    /// Normal priority - default
    /// </summary>
    Normal = 1,
    
    /// <summary>
    /// High priority - should be processed soon
    /// </summary>
    High = 2,
    
    /// <summary>
    /// Urgent priority - process immediately
    /// </summary>
    Urgent = 3
}
