namespace SlipVerification.Application.DTOs.Reports;

/// <summary>
/// Daily report DTO
/// </summary>
public class DailyReportDto
{
    /// <summary>
    /// Report date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Total transactions for the day
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Total revenue for the day
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
    /// Transaction details
    /// </summary>
    public List<ReportTransactionDto> Transactions { get; set; } = new();
}

/// <summary>
/// Monthly report DTO
/// </summary>
public class MonthlyReportDto
{
    /// <summary>
    /// Report year
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Report month
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Month name
    /// </summary>
    public string MonthName { get; set; } = string.Empty;

    /// <summary>
    /// Total transactions for the month
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Total revenue for the month
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Success rate percentage
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Daily breakdown
    /// </summary>
    public List<DailySummaryDto> DailyBreakdown { get; set; } = new();

    /// <summary>
    /// Bank breakdown
    /// </summary>
    public List<BankSummaryDto> BankBreakdown { get; set; } = new();
}

/// <summary>
/// Transaction detail for reports
/// </summary>
public class ReportTransactionDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Daily summary for monthly report
/// </summary>
public class DailySummaryDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Bank summary for reports
/// </summary>
public class BankSummaryDto
{
    public string BankName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Custom report request DTO
/// </summary>
public class CustomReportRequestDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Status { get; set; }
    public string? BankName { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}

/// <summary>
/// Export options DTO
/// </summary>
public class ExportOptionsDto
{
    public string Format { get; set; } = "excel"; // excel, csv, pdf
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> Columns { get; set; } = new();
}
