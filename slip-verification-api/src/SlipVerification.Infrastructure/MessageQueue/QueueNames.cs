namespace SlipVerification.Infrastructure.MessageQueue;

/// <summary>
/// Static class defining all queue names used in the system
/// </summary>
public static class QueueNames
{
    public const string SlipProcessing = "slip-processing-queue";
    public const string Notifications = "notifications-queue";
    public const string EmailNotifications = "email-notifications-queue";
    public const string PushNotifications = "push-notifications-queue";
    public const string Reports = "reports-queue";
    public const string DeadLetter = "dead-letter-queue";
}

/// <summary>
/// Static class defining exchange names
/// </summary>
public static class ExchangeNames
{
    public const string SlipVerification = "slip-verification-exchange";
    public const string DeadLetter = "dead-letter-exchange";
}

/// <summary>
/// Static class defining routing keys
/// </summary>
public static class RoutingKeys
{
    public const string SlipProcessing = "slip.processing";
    public const string SlipVerified = "slip.verified";
    public const string SlipRejected = "slip.rejected";
    public const string NotificationEmail = "notification.email";
    public const string NotificationPush = "notification.push";
    public const string ReportGeneration = "report.generation";
}
