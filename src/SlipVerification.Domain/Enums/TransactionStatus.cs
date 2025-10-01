namespace SlipVerification.Domain.Enums;

/// <summary>
/// Represents the status of a transaction
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction is pending
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Transaction is processing
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Transaction completed successfully
    /// </summary>
    Success = 2,
    
    /// <summary>
    /// Transaction failed
    /// </summary>
    Failed = 3,
    
    /// <summary>
    /// Transaction was cancelled
    /// </summary>
    Cancelled = 4,
    
    /// <summary>
    /// Transaction was refunded
    /// </summary>
    Refunded = 5
}
