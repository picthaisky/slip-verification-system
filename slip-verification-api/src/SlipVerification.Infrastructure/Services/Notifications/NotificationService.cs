using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SlipVerification.Application.DTOs.Notifications;
using SlipVerification.Application.Interfaces.Notifications;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;
using SlipVerification.Infrastructure.Data;

namespace SlipVerification.Infrastructure.Services.Notifications;

/// <summary>
/// Main notification service orchestrating notification sending
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly IRateLimiter _rateLimiter;
    private readonly ITemplateEngine _templateEngine;
    private readonly AsyncRetryPolicy<NotificationResult> _retryPolicy;

    public NotificationService(
        ILogger<NotificationService> logger,
        ApplicationDbContext context,
        IEnumerable<INotificationChannel> channels,
        IRateLimiter rateLimiter,
        ITemplateEngine templateEngine)
    {
        _logger = logger;
        _context = context;
        _channels = channels;
        _rateLimiter = rateLimiter;
        _templateEngine = templateEngine;
        _retryPolicy = CreateRetryPolicy();
    }

    public async Task<NotificationResult> SendNotificationAsync(NotificationMessage message)
    {
        try
        {
            // Check rate limiting
            var rateLimitKey = $"user:{message.UserId}";
            if (!await _rateLimiter.IsAllowedAsync(rateLimitKey, message.Channel))
            {
                return NotificationResult.CreateFailure("Rate limit exceeded");
            }

            // Render template if template code is provided
            if (!string.IsNullOrEmpty(message.TemplateCode) && message.Placeholders != null)
            {
                var (subject, body) = await _templateEngine.RenderNotificationTemplateAsync(
                    message.TemplateCode,
                    message.Channel,
                    message.Placeholders,
                    message.Language);

                if (!string.IsNullOrEmpty(subject))
                    message.Title = subject;
                if (!string.IsNullOrEmpty(body))
                    message.Message = body;
            }

            // Create notification record
            var notification = new Notification
            {
                UserId = message.UserId,
                Type = message.Type,
                Title = message.Title,
                Message = message.Message,
                Channel = message.Channel.ToString(),
                Status = NotificationStatus.Processing.ToString(),
                Priority = message.Priority,
                Data = message.Data != null ? JsonSerializer.Serialize(message.Data) : null,
                RetryCount = 0,
                MaxRetryCount = 3
            };

            _context.Set<Notification>().Add(notification);
            await _context.SaveChangesAsync();

            // Get appropriate channel
            var channel = _channels.FirstOrDefault(c => c.SupportsChannel(message.Channel));
            if (channel == null)
            {
                await UpdateNotificationStatus(notification.Id, NotificationStatus.Failed, "No channel implementation found");
                return NotificationResult.CreateFailure("No channel implementation found");
            }

            // Send notification with retry policy
            var result = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await channel.SendAsync(message);
            });

            // Update notification status
            if (result.Success)
            {
                await UpdateNotificationStatus(notification.Id, NotificationStatus.Sent, null, result.ProviderMessageId);
                await _rateLimiter.RecordRequestAsync(rateLimitKey, message.Channel);
                result.NotificationId = notification.Id;
            }
            else
            {
                await UpdateNotificationStatus(notification.Id, NotificationStatus.Failed, result.ErrorMessage);
            }

            // Send webhook callback if provided
            if (!string.IsNullOrEmpty(message.CallbackUrl))
            {
                _ = SendWebhookCallbackAsync(message.CallbackUrl, notification.Id, result);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", message.UserId);
            return NotificationResult.CreateFailure(ex.Message);
        }
    }

    public async Task<Guid> QueueNotificationAsync(NotificationMessage message)
    {
        var notification = new Notification
        {
            UserId = message.UserId,
            Type = message.Type,
            Title = message.Title,
            Message = message.Message,
            Channel = message.Channel.ToString(),
            Status = NotificationStatus.Pending.ToString(),
            Priority = message.Priority,
            Data = message.Data != null ? JsonSerializer.Serialize(message.Data) : null,
            RetryCount = 0,
            MaxRetryCount = 3
        };

        _context.Set<Notification>().Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification {NotificationId} queued for user {UserId}", 
            notification.Id, message.UserId);

        return notification.Id;
    }

    public async Task<NotificationDto?> GetNotificationAsync(Guid id)
    {
        var notification = await _context.Set<Notification>()
            .Where(n => n.Id == id && !n.IsDeleted)
            .FirstOrDefaultAsync();

        if (notification == null)
            return null;

        return MapToDto(notification);
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var notifications = await _context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return notifications.Select(MapToDto).ToList();
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _context.Set<Notification>().FindAsync(notificationId);
        if (notification != null && notification.ReadAt == null)
        {
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
        }
    }

    public async Task<NotificationResult> RetryNotificationAsync(Guid notificationId)
    {
        var notification = await _context.Set<Notification>().FindAsync(notificationId);
        if (notification == null)
        {
            return NotificationResult.CreateFailure("Notification not found");
        }

        if (notification.RetryCount >= notification.MaxRetryCount)
        {
            return NotificationResult.CreateFailure("Maximum retry attempts reached");
        }

        // Reconstruct notification message
        var message = new NotificationMessage
        {
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            Channel = Enum.Parse<NotificationChannel>(notification.Channel),
            Priority = notification.Priority,
            Data = !string.IsNullOrEmpty(notification.Data) 
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Data) 
                : null
        };

        notification.RetryCount++;
        notification.Status = NotificationStatus.Retrying.ToString();
        await _context.SaveChangesAsync();

        return await SendNotificationAsync(message);
    }

    private async Task UpdateNotificationStatus(Guid notificationId, NotificationStatus status, 
        string? errorMessage = null, string? providerMessageId = null)
    {
        var notification = await _context.Set<Notification>().FindAsync(notificationId);
        if (notification != null)
        {
            notification.Status = status.ToString();
            notification.ErrorMessage = errorMessage;
            
            if (status == NotificationStatus.Sent)
            {
                notification.SentAt = DateTime.UtcNow;
            }
            
            if (status == NotificationStatus.Failed && notification.RetryCount < notification.MaxRetryCount)
            {
                notification.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, notification.RetryCount));
            }

            await _context.SaveChangesAsync();
        }
    }

    private async Task SendWebhookCallbackAsync(string callbackUrl, Guid notificationId, NotificationResult result)
    {
        try
        {
            using var httpClient = new HttpClient();
            var payload = new
            {
                notificationId,
                success = result.Success,
                errorMessage = result.ErrorMessage,
                sentAt = result.SentAt
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            await httpClient.PostAsync(callbackUrl, content);
            _logger.LogInformation("Webhook callback sent to {CallbackUrl} for notification {NotificationId}",
                callbackUrl, notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook callback to {CallbackUrl}", callbackUrl);
        }
    }

    private NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            Channel = notification.Channel,
            Status = notification.Status,
            Priority = notification.Priority.ToString(),
            RetryCount = notification.RetryCount,
            SentAt = notification.SentAt,
            ReadAt = notification.ReadAt,
            ErrorMessage = notification.ErrorMessage,
            CreatedAt = notification.CreatedAt
        };
    }

    private AsyncRetryPolicy<NotificationResult> CreateRetryPolicy()
    {
        return Policy<NotificationResult>
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .OrResult(r => !r.Success && (r.ErrorMessage?.Contains("timeout", StringComparison.OrdinalIgnoreCase) ?? false))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} after {Delay}s due to: {Reason}",
                        retryCount, timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.ErrorMessage);
                });
    }
}
