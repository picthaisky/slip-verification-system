using SlipVerification.Domain.Common;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Domain.Entities;

/// <summary>
/// Represents a payment transaction in the system
/// </summary>
public class Transaction : BaseEntity
{
    /// <summary>
    /// Gets or sets the order ID
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Gets or sets the slip verification ID
    /// </summary>
    public Guid SlipVerificationId { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets when the transaction was processed
    /// </summary>
    public DateTime ProcessedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction status
    /// </summary>
    public TransactionStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction reference number
    /// </summary>
    public string? TransactionReference { get; set; }
    
    /// <summary>
    /// Gets or sets the payment method
    /// </summary>
    public string? PaymentMethod { get; set; }
    
    /// <summary>
    /// Gets or sets additional transaction metadata
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Gets or sets transaction notes
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Navigation property for order
    /// </summary>
    public virtual Order Order { get; set; } = null!;
    
    /// <summary>
    /// Navigation property for slip verification
    /// </summary>
    public virtual SlipVerification SlipVerification { get; set; } = null!;
}
