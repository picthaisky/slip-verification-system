namespace SlipVerification.Domain.Enums;

/// <summary>
/// Represents the status of an order
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been created but payment not yet received
    /// </summary>
    PendingPayment = 0,
    
    /// <summary>
    /// Payment has been received and verified
    /// </summary>
    Paid = 1,
    
    /// <summary>
    /// Order is being processed
    /// </summary>
    Processing = 2,
    
    /// <summary>
    /// Order has been completed
    /// </summary>
    Completed = 3,
    
    /// <summary>
    /// Order has been cancelled
    /// </summary>
    Cancelled = 4,
    
    /// <summary>
    /// Order has been refunded
    /// </summary>
    Refunded = 5
}
