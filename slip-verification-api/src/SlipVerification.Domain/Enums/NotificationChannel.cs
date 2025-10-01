namespace SlipVerification.Domain.Enums;

/// <summary>
/// Represents notification delivery channels
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// LINE Notify channel
    /// </summary>
    LINE = 0,
    
    /// <summary>
    /// Email channel (SMTP/SendGrid)
    /// </summary>
    EMAIL = 1,
    
    /// <summary>
    /// Push notification channel (FCM)
    /// </summary>
    PUSH = 2,
    
    /// <summary>
    /// SMS channel (Twilio)
    /// </summary>
    SMS = 3
}
