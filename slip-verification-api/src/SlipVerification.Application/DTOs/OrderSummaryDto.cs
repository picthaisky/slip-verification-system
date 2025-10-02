using SlipVerification.Domain.Enums;

namespace SlipVerification.Application.DTOs;

/// <summary>
/// Lightweight DTO for order summaries (optimized for list views)
/// </summary>
public class OrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Description { get; set; }
}
