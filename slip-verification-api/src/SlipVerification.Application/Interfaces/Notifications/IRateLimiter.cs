using SlipVerification.Domain.Enums;

namespace SlipVerification.Application.Interfaces.Notifications;

/// <summary>
/// Interface for rate limiting notification sending
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Checks if a request is allowed under the rate limit
    /// </summary>
    /// <param name="key">The rate limit key (e.g., channel type, user ID)</param>
    /// <param name="channel">The notification channel</param>
    /// <returns>True if allowed, false if rate limited</returns>
    Task<bool> IsAllowedAsync(string key, NotificationChannel channel);
    
    /// <summary>
    /// Records a request for rate limiting
    /// </summary>
    /// <param name="key">The rate limit key</param>
    /// <param name="channel">The notification channel</param>
    Task RecordRequestAsync(string key, NotificationChannel channel);
    
    /// <summary>
    /// Gets the remaining requests allowed in the current window
    /// </summary>
    /// <param name="key">The rate limit key</param>
    /// <param name="channel">The notification channel</param>
    /// <returns>Number of remaining requests</returns>
    Task<int> GetRemainingRequestsAsync(string key, NotificationChannel channel);
}
