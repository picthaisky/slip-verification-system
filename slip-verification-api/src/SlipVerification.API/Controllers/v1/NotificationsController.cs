using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlipVerification.Application.DTOs.Notifications;
using SlipVerification.Application.Interfaces.Notifications;

namespace SlipVerification.API.Controllers.v1;

/// <summary>
/// Notification management endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly INotificationQueueService _queueService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        INotificationQueueService queueService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _queueService = queueService;
        _logger = logger;
    }

    /// <summary>
    /// Send a notification immediately
    /// </summary>
    /// <param name="request">Notification request</param>
    /// <returns>Notification result</returns>
    [HttpPost("send")]
    [ProducesResponseType(typeof(NotificationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        try
        {
            var message = new NotificationMessage
            {
                UserId = request.UserId,
                Channel = request.Channel,
                Priority = request.Priority,
                Title = request.Title,
                Message = request.Message,
                TemplateCode = request.TemplateCode,
                Placeholders = request.Placeholders,
                Data = request.Data
            };

            var result = await _notificationService.SendNotificationAsync(message);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Queue a notification for asynchronous processing
    /// </summary>
    /// <param name="request">Notification request</param>
    /// <returns>Notification ID</returns>
    [HttpPost("queue")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> QueueNotification([FromBody] SendNotificationRequest request)
    {
        try
        {
            var message = new NotificationMessage
            {
                UserId = request.UserId,
                Channel = request.Channel,
                Priority = request.Priority,
                Title = request.Title,
                Message = request.Message,
                TemplateCode = request.TemplateCode,
                Placeholders = request.Placeholders,
                Data = request.Data
            };

            await _queueService.EnqueueAsync(message);
            
            return Accepted(new { message = "Notification queued for processing" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing notification");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get notification by ID
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Notification details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotification(Guid id)
    {
        var notification = await _notificationService.GetNotificationAsync(id);
        
        if (notification == null)
        {
            return NotFound();
        }

        return Ok(notification);
    }

    /// <summary>
    /// Get notifications for the current user
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of notifications</returns>
    [HttpGet("my")]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // In a real implementation, get user ID from claims
        var userId = GetCurrentUserId();
        
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
        
        return Ok(notifications);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Success status</returns>
    [HttpPut("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Retry a failed notification
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Notification result</returns>
    [HttpPost("{id}/retry")]
    [ProducesResponseType(typeof(NotificationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RetryNotification(Guid id)
    {
        try
        {
            var result = await _notificationService.RetryNotificationAsync(id);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying notification {NotificationId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    private Guid GetCurrentUserId()
    {
        // Extract user ID from JWT claims
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        // Fallback for development
        return Guid.Empty;
    }
}
