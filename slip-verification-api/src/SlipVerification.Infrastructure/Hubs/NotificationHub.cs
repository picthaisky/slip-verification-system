using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SlipVerification.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Join user-specific group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            _logger.LogInformation(
                "User {UserId} connected with connection {ConnectionId}",
                userId,
                Context.ConnectionId
            );
        }
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            _logger.LogInformation(
                "User {UserId} disconnected from connection {ConnectionId}",
                userId,
                Context.ConnectionId
            );
        }

        if (exception != null)
        {
            _logger.LogError(exception, "Error during disconnection");
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client can call this to join order-specific room
    /// </summary>
    public async Task JoinOrderRoom(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        _logger.LogInformation(
            "Connection {ConnectionId} joined order room {OrderId}",
            Context.ConnectionId, 
            orderId
        );
    }

    /// <summary>
    /// Client can call this to leave order-specific room
    /// </summary>
    public async Task LeaveOrderRoom(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        _logger.LogInformation(
            "Connection {ConnectionId} left order room {OrderId}",
            Context.ConnectionId, 
            orderId
        );
    }
}
