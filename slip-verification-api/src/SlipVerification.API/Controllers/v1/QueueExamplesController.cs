using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlipVerification.Application.DTOs.MessageQueue;
using SlipVerification.Application.Interfaces.MessageQueue;
using SlipVerification.Infrastructure.MessageQueue;

namespace SlipVerification.API.Controllers.v1;

/// <summary>
/// Example controller demonstrating message queue usage
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class QueueExamplesController : ControllerBase
{
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<QueueExamplesController> _logger;

    public QueueExamplesController(
        IMessagePublisher publisher,
        ILogger<QueueExamplesController> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Publish a slip processing message
    /// </summary>
    [HttpPost("slip/process")]
    public async Task<IActionResult> PublishSlipProcessing(
        [FromBody] SlipProcessingRequest request)
    {
        var message = new SlipProcessingMessage
        {
            SlipId = request.SlipId,
            UserId = request.UserId,
            ImageUrl = request.ImageUrl,
            ProcessingType = request.ProcessingType ?? "OCR"
        };

        await _publisher.PublishAsync(QueueNames.SlipProcessing, message);

        _logger.LogInformation(
            "Queued slip {SlipId} for processing",
            message.SlipId
        );

        return Accepted(new
        {
            message = "Slip queued for processing",
            messageId = message.MessageId,
            slipId = message.SlipId
        });
    }

    /// <summary>
    /// Publish a notification message
    /// </summary>
    [HttpPost("notification/send")]
    public async Task<IActionResult> PublishNotification(
        [FromBody] NotificationRequest request)
    {
        var message = new NotificationQueueMessage
        {
            UserId = request.UserId,
            Channel = request.Channel,
            Title = request.Title,
            Message = request.Message,
            Priority = request.Priority,
            Data = request.Data
        };

        await _publisher.PublishAsync(QueueNames.Notifications, message);

        _logger.LogInformation(
            "Queued notification for user {UserId} via channel {Channel}",
            message.UserId,
            message.Channel
        );

        return Accepted(new
        {
            message = "Notification queued",
            messageId = message.MessageId
        });
    }

    /// <summary>
    /// Publish an email notification
    /// </summary>
    [HttpPost("notification/email")]
    public async Task<IActionResult> PublishEmailNotification(
        [FromBody] EmailNotificationRequest request)
    {
        var message = new EmailNotificationMessage
        {
            To = request.To,
            Subject = request.Subject,
            Body = request.Body,
            IsHtml = request.IsHtml,
            Cc = request.Cc,
            Bcc = request.Bcc
        };

        await _publisher.PublishAsync(
            ExchangeNames.SlipVerification,
            RoutingKeys.NotificationEmail,
            message
        );

        _logger.LogInformation(
            "Queued email notification to {To}",
            message.To
        );

        return Accepted(new
        {
            message = "Email queued for sending",
            messageId = message.MessageId
        });
    }

    /// <summary>
    /// Publish a report generation request
    /// </summary>
    [HttpPost("report/generate")]
    public async Task<IActionResult> PublishReportGeneration(
        [FromBody] ReportGenerationRequest request)
    {
        var message = new ReportGenerationMessage
        {
            ReportId = Guid.NewGuid(),
            UserId = request.UserId,
            ReportType = request.ReportType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Parameters = request.Parameters
        };

        await _publisher.PublishAsync(QueueNames.Reports, message);

        _logger.LogInformation(
            "Queued report {ReportId} of type {ReportType} for user {UserId}",
            message.ReportId,
            message.ReportType,
            message.UserId
        );

        return Accepted(new
        {
            message = "Report queued for generation",
            messageId = message.MessageId,
            reportId = message.ReportId
        });
    }
}

// Request DTOs

public record SlipProcessingRequest(
    Guid SlipId,
    Guid UserId,
    string ImageUrl,
    string? ProcessingType
);

public record NotificationRequest(
    Guid UserId,
    string Channel,
    string Title,
    string Message,
    int Priority,
    Dictionary<string, object>? Data
);

public record EmailNotificationRequest(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true,
    List<string>? Cc = null,
    List<string>? Bcc = null
);

public record ReportGenerationRequest(
    Guid UserId,
    string ReportType,
    DateTime StartDate,
    DateTime EndDate,
    Dictionary<string, object>? Parameters
);
