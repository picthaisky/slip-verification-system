namespace SlipVerification.Application.DTOs.Dashboard;

/// <summary>
/// Recent activity item DTO
/// </summary>
public class RecentActivityDto
{
    /// <summary>
    /// Activity ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Activity type (SlipVerified, OrderCreated, PaymentReceived, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Activity description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Related entity ID (Order ID, Slip ID, etc.)
    /// </summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// Activity status (Success, Pending, Failed)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Amount involved (if applicable)
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Activity timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Time ago string (e.g., "2 hours ago")
    /// </summary>
    public string TimeAgo { get; set; } = string.Empty;

    /// <summary>
    /// Icon name for display
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Color for the activity status
    /// </summary>
    public string Color { get; set; } = string.Empty;
}
