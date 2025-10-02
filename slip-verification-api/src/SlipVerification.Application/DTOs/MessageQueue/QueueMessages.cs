namespace SlipVerification.Application.DTOs.MessageQueue;

/// <summary>
/// Base message for queue processing
/// </summary>
public abstract class BaseQueueMessage
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Message for slip processing queue
/// </summary>
public class SlipProcessingMessage : BaseQueueMessage
{
    public Guid SlipId { get; set; }
    public Guid UserId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ProcessingType { get; set; } = "OCR";
}

/// <summary>
/// Message for notification queue
/// </summary>
public class NotificationQueueMessage : BaseQueueMessage
{
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Message for report generation queue
/// </summary>
public class ReportGenerationMessage : BaseQueueMessage
{
    public Guid ReportId { get; set; }
    public Guid UserId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Message for email notifications
/// </summary>
public class EmailNotificationMessage : BaseQueueMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
}

/// <summary>
/// Message for push notifications
/// </summary>
public class PushNotificationMessage : BaseQueueMessage
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string>? Data { get; set; }
    public string? ImageUrl { get; set; }
}
