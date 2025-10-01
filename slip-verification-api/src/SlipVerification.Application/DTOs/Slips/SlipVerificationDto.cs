namespace SlipVerification.Application.DTOs.Slips;

/// <summary>
/// DTO for slip verification details
/// </summary>
public class SlipVerificationDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? ImageHash { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? TransactionDate { get; set; }
    public TimeSpan? TransactionTime { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RawOcrText { get; set; }
    public decimal? OcrConfidence { get; set; }
    public string? VerificationNotes { get; set; }
    public Guid? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
