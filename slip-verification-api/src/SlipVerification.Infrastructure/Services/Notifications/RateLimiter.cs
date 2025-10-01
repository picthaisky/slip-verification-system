using Microsoft.Extensions.Logging;
using SlipVerification.Application.Interfaces;
using SlipVerification.Application.Interfaces.Notifications;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Infrastructure.Services.Notifications;

/// <summary>
/// Rate limiter implementation using Redis
/// </summary>
public class RateLimiter : IRateLimiter
{
    private readonly ILogger<RateLimiter> _logger;
    private readonly ICacheService _cacheService;

    // Rate limit configurations per channel
    private static readonly Dictionary<NotificationChannel, (int Limit, TimeSpan Window)> RateLimits = new()
    {
        { NotificationChannel.LINE, (1000, TimeSpan.FromHours(1)) },      // 1000 calls/hour
        { NotificationChannel.EMAIL, (100, TimeSpan.FromMinutes(1)) },    // 100 emails/minute
        { NotificationChannel.SMS, (10, TimeSpan.FromMinutes(1)) },       // 10 SMS/minute
        { NotificationChannel.PUSH, (500, TimeSpan.FromMinutes(1)) }      // 500 notifications/minute
    };

    public RateLimiter(
        ILogger<RateLimiter> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<bool> IsAllowedAsync(string key, NotificationChannel channel)
    {
        var rateLimitKey = GetRateLimitKey(key, channel);
        var (limit, window) = RateLimits[channel];

        var currentCount = await GetCurrentCountAsync(rateLimitKey);
        
        if (currentCount >= limit)
        {
            _logger.LogWarning("Rate limit exceeded for key {Key} on channel {Channel}. Current: {Current}, Limit: {Limit}",
                key, channel, currentCount, limit);
            return false;
        }

        return true;
    }

    public async Task RecordRequestAsync(string key, NotificationChannel channel)
    {
        var rateLimitKey = GetRateLimitKey(key, channel);
        var (_, window) = RateLimits[channel];

        var currentCount = await GetCurrentCountAsync(rateLimitKey);
        var newCount = currentCount + 1;

        await _cacheService.SetAsync(rateLimitKey, newCount.ToString(), window);
        
        _logger.LogDebug("Recorded request for key {Key} on channel {Channel}. Count: {Count}",
            key, channel, newCount);
    }

    public async Task<int> GetRemainingRequestsAsync(string key, NotificationChannel channel)
    {
        var rateLimitKey = GetRateLimitKey(key, channel);
        var (limit, _) = RateLimits[channel];

        var currentCount = await GetCurrentCountAsync(rateLimitKey);
        var remaining = Math.Max(0, limit - currentCount);

        return remaining;
    }

    private string GetRateLimitKey(string key, NotificationChannel channel)
    {
        return $"ratelimit:{channel.ToString().ToLower()}:{key}";
    }

    private async Task<int> GetCurrentCountAsync(string key)
    {
        var value = await _cacheService.GetAsync<string>(key);
        return int.TryParse(value, out var count) ? count : 0;
    }
}
