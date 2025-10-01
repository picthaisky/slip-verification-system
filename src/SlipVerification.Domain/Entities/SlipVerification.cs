using SlipVerification.Domain.Common;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Domain.Entities;

/// <summary>
/// Represents a payment slip verification record
/// </summary>
public class SlipVerification : BaseEntity
{
    /// <summary>
    /// Gets or sets the order ID this slip belongs to
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Gets or sets the path to the uploaded slip image
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount on the slip
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction date from the slip
    /// </summary>
    public DateTime TransactionDate { get; set; }
    
    /// <summary>
    /// Gets or sets the reference number from the slip
    /// </summary>
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the bank name from the slip
    /// </summary>
    public string? BankName { get; set; }
    
    /// <summary>
    /// Gets or sets the sender account number or name
    /// </summary>
    public string? SenderAccount { get; set; }
    
    /// <summary>
    /// Gets or sets the receiver account number or name
    /// </summary>
    public string? ReceiverAccount { get; set; }
    
    /// <summary>
    /// Gets or sets the verification status
    /// </summary>
    public VerificationStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the raw OCR text extracted from the slip
    /// </summary>
    public string? RawOcrText { get; set; }
    
    /// <summary>
    /// Gets or sets the OCR confidence score (0-1)
    /// </summary>
    public decimal? OcrConfidence { get; set; }
    
    /// <summary>
    /// Gets or sets the verification notes or reason for failure
    /// </summary>
    public string? VerificationNotes { get; set; }
    
    /// <summary>
    /// Gets or sets when the slip was verified
    /// </summary>
    public DateTime? VerifiedAt { get; set; }
    
    /// <summary>
    /// Navigation property for order
    /// </summary>
    public virtual Order Order { get; set; } = null!;
    
    /// <summary>
    /// Navigation property for transactions
    /// </summary>
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
