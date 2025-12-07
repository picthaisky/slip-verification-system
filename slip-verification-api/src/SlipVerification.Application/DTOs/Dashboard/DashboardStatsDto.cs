namespace SlipVerification.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard statistics DTO
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// Total number of transactions
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Total revenue amount
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Number of verified transactions
    /// </summary>
    public int VerifiedCount { get; set; }

    /// <summary>
    /// Number of pending transactions
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Number of rejected transactions
    /// </summary>
    public int RejectedCount { get; set; }

    /// <summary>
    /// Success rate percentage (0-100)
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Average processing time in seconds
    /// </summary>
    public double AverageProcessingTime { get; set; }

    /// <summary>
    /// Transactions today
    /// </summary>
    public int TodayTransactions { get; set; }

    /// <summary>
    /// Revenue today
    /// </summary>
    public decimal TodayRevenue { get; set; }
}
