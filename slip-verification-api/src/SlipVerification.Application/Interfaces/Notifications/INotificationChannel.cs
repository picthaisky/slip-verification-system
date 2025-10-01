using SlipVerification.Application.DTOs.Notifications;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Application.Interfaces.Notifications;

/// <summary>
/// Interface for notification channel implementations
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// Sends a notification through this channel
    /// </summary>
    /// <param name="message">The notification message to send</param>
    /// <returns>Result of the send operation</returns>
    Task<NotificationResult> SendAsync(NotificationMessage message);
    
    /// <summary>
    /// Checks if this channel supports the given notification channel type
    /// </summary>
    /// <param name="channel">The channel type to check</param>
    /// <returns>True if supported, false otherwise</returns>
    bool SupportsChannel(NotificationChannel channel);
    
    /// <summary>
    /// Gets the channel type this implementation handles
    /// </summary>
    NotificationChannel ChannelType { get; }
}
