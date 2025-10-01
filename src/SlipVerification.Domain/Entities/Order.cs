using SlipVerification.Domain.Common;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Domain.Entities;

/// <summary>
/// Represents an order in the system
/// </summary>
public class Order : BaseEntity
{
    /// <summary>
    /// Gets or sets the order number (unique identifier for customers)
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the order amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the order status
    /// </summary>
    public OrderStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the order was paid
    /// </summary>
    public DateTime? PaidAt { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID who owns this order
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the order description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets additional notes
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Gets or sets the expected payment date
    /// </summary>
    public DateTime? ExpectedPaymentDate { get; set; }
    
    /// <summary>
    /// Navigation property for user
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// Navigation property for slip verifications
    /// </summary>
    public virtual ICollection<SlipVerification> SlipVerifications { get; set; } = new List<SlipVerification>();
    
    /// <summary>
    /// Navigation property for transactions
    /// </summary>
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
