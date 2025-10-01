namespace SlipVerification.Application.DTOs.Transactions;

/// <summary>
/// DTO for transaction details
/// </summary>
public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid SlipVerificationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
