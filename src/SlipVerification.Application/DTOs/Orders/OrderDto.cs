namespace SlipVerification.Application.DTOs.Orders;

/// <summary>
/// DTO for order details
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public Guid UserId { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime? ExpectedPaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
