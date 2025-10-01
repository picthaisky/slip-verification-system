using SlipVerification.Application.DTOs.Notifications;

namespace SlipVerification.Application.Interfaces.Notifications;

/// <summary>
/// Interface for notification queue processing
/// </summary>
public interface INotificationQueueService
{
    /// <summary>
    /// Enqueues a notification message for processing
    /// </summary>
    Task EnqueueAsync(NotificationMessage message);
    
    /// <summary>
    /// Starts consuming messages from the queue
    /// </summary>
    Task StartConsumingAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Processes a single notification message
    /// </summary>
    Task ProcessNotificationAsync(NotificationMessage message);
}
