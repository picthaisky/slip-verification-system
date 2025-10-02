using SlipVerification.Domain.Entities;

namespace SlipVerification.Application.DTOs;

/// <summary>
/// Detailed DTO for order with related entities (optimized by splitting queries)
/// </summary>
public class OrderDetailDto
{
    public Order Order { get; set; } = null!;
    public IEnumerable<SlipVerification.Domain.Entities.SlipVerification> Slips { get; set; } = Enumerable.Empty<SlipVerification.Domain.Entities.SlipVerification>();
    public IEnumerable<Transaction> Transactions { get; set; } = Enumerable.Empty<Transaction>();
}
