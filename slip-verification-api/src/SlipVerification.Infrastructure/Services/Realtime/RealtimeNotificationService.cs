using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SlipVerification.Application.Interfaces;
using SlipVerification.Infrastructure.Hubs;

namespace SlipVerification.Infrastructure.Services.Realtime;

/// <summary>
/// Real-time notification service implementation using SignalR
/// </summary>
public class RealtimeNotificationService : IRealtimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RealtimeNotificationService> _logger;

    public RealtimeNotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<RealtimeNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Notify a specific user about slip verification result
    /// </summary>
    public async Task NotifySlipVerifiedAsync(Guid userId, SlipVerificationResult result)
    {
        try
        {
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("SlipVerified", new
                {
                    orderId = result.OrderId,
                    amount = result.Amount,
                    status = result.Status,
                    timestamp = DateTime.UtcNow
                });

            _logger.LogInformation(
                "Sent slip verification notification to user {UserId} for order {OrderId}",
                userId,
                result.OrderId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending slip verification notification to user {UserId}",
                userId
            );
        }
    }

    /// <summary>
    /// Notify all users in an order room about payment received
    /// </summary>
    public async Task NotifyPaymentReceivedAsync(Guid orderId, PaymentNotification notification)
    {
        try
        {
            await _hubContext.Clients
                .Group($"order_{orderId}")
                .SendAsync("PaymentReceived", notification);

            _logger.LogInformation(
                "Sent payment received notification for order {OrderId}",
                orderId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending payment received notification for order {OrderId}",
                orderId
            );
        }
    }

    /// <summary>
    /// Broadcast a system message to all connected users
    /// </summary>
    public async Task BroadcastSystemMessageAsync(string message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("SystemMessage", new
            {
                message,
                timestamp = DateTime.UtcNow,
                type = "info"
            });

            _logger.LogInformation("Broadcasted system message: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting system message");
        }
    }
}
