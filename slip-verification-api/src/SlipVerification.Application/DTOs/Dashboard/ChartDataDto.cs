namespace SlipVerification.Application.DTOs.Dashboard;

/// <summary>
/// Chart data DTO for dashboard visualizations
/// </summary>
public class ChartDataDto
{
    /// <summary>
    /// Chart labels (dates, months, etc.)
    /// </summary>
    public List<string> Labels { get; set; } = new();

    /// <summary>
    /// Chart datasets
    /// </summary>
    public List<ChartDatasetDto> Datasets { get; set; } = new();
}

/// <summary>
/// Chart dataset DTO
/// </summary>
public class ChartDatasetDto
{
    /// <summary>
    /// Dataset label
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Data values
    /// </summary>
    public List<decimal> Data { get; set; } = new();

    /// <summary>
    /// Background color
    /// </summary>
    public string BackgroundColor { get; set; } = string.Empty;

    /// <summary>
    /// Border color
    /// </summary>
    public string BorderColor { get; set; } = string.Empty;
}

/// <summary>
/// Transaction summary by period
/// </summary>
public class TransactionSummaryDto
{
    /// <summary>
    /// Period label (date or month name)
    /// </summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// Transaction count
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Total amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Verified count
    /// </summary>
    public int VerifiedCount { get; set; }

    /// <summary>
    /// Pending count
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Rejected count
    /// </summary>
    public int RejectedCount { get; set; }
}
