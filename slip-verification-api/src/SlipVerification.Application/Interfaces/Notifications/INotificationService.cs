using SlipVerification.Application.DTOs.Notifications;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Application.Interfaces.Notifications;

/// <summary>
/// Interface for the main notification service
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification through the specified channel
    /// </summary>
    Task<NotificationResult> SendNotificationAsync(NotificationMessage message);
    
    /// <summary>
    /// Queues a notification for later processing
    /// </summary>
    Task<Guid> QueueNotificationAsync(NotificationMessage message);
    
    /// <summary>
    /// Gets notification by ID
    /// </summary>
    Task<NotificationDto?> GetNotificationAsync(Guid id);
    
    /// <summary>
    /// Gets notifications for a user
    /// </summary>
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Marks notification as read
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId);
    
    /// <summary>
    /// Retries a failed notification
    /// </summary>
    Task<NotificationResult> RetryNotificationAsync(Guid notificationId);
}
