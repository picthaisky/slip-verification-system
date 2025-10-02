namespace SlipVerification.Application.Interfaces;

/// <summary>
/// Interface for real-time notification service using SignalR
/// </summary>
public interface IRealtimeNotificationService
{
    /// <summary>
    /// Notify a specific user about slip verification result
    /// </summary>
    Task NotifySlipVerifiedAsync(Guid userId, SlipVerificationResult result);

    /// <summary>
    /// Notify all users in an order room about payment received
    /// </summary>
    Task NotifyPaymentReceivedAsync(Guid orderId, PaymentNotification notification);

    /// <summary>
    /// Broadcast a system message to all connected users
    /// </summary>
    Task BroadcastSystemMessageAsync(string message);
}

/// <summary>
/// Slip verification result for real-time notification
/// </summary>
public class SlipVerificationResult
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Payment notification for real-time updates
/// </summary>
public class PaymentNotification
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime Timestamp { get; set; }
}
