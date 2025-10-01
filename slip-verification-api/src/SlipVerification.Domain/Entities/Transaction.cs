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
    /// Gets or sets the slip verification ID (nullable)
    /// </summary>
    public Guid? SlipVerificationId { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction type
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the transaction status
    /// </summary>
    public TransactionStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the payment method
    /// </summary>
    public string? PaymentMethod { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets additional transaction metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Gets or sets when the transaction was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Navigation property for user
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Navigation property for order
    /// </summary>
    public virtual Order Order { get; set; } = null!;
    
    /// <summary>
    /// Navigation property for slip verification
    /// </summary>
    public virtual SlipVerification? SlipVerification { get; set; }
}
