using MediatR;
using Microsoft.EntityFrameworkCore;
using SlipVerification.Application.DTOs.Reports;
using SlipVerification.Infrastructure.Data;
using System.Globalization;

namespace SlipVerification.Application.Features.Reports.Queries;

/// <summary>
/// Query to get monthly report
/// </summary>
public class GetMonthlyReportQuery : IRequest<MonthlyReportDto>
{
    /// <summary>
    /// Report year
    /// </summary>
    public int Year { get; set; } = DateTime.UtcNow.Year;

    /// <summary>
    /// Report month (1-12)
    /// </summary>
    public int Month { get; set; } = DateTime.UtcNow.Month;
}

/// <summary>
/// Handler for GetMonthlyReportQuery
/// </summary>
public class GetMonthlyReportQueryHandler : IRequestHandler<GetMonthlyReportQuery, MonthlyReportDto>
{
    private readonly ApplicationDbContext _context;

    public GetMonthlyReportQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MonthlyReportDto> Handle(GetMonthlyReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateTime(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1);

        var slips = await _context.SlipVerifications
            .AsNoTracking()
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt < endDate)
            .ToListAsync(cancellationToken);

        var verifiedCount = slips.Count(s => s.Status == Domain.Enums.SlipVerificationStatus.Verified);
        var successRate = slips.Count > 0 
            ? Math.Round((decimal)verifiedCount / slips.Count * 100, 2) 
            : 0;

        // Daily breakdown
        var dailyBreakdown = slips
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new DailySummaryDto
            {
                Date = g.Key,
                Count = g.Count(),
                Amount = g.Sum(s => s.Amount)
            })
            .OrderBy(d => d.Date)
            .ToList();

        // Bank breakdown
        var totalAmount = slips.Sum(s => s.Amount);
        var bankBreakdown = slips
            .GroupBy(s => s.BankName ?? "Unknown")
            .Select(g => new BankSummaryDto
            {
                BankName = g.Key,
                TransactionCount = g.Count(),
                TotalAmount = g.Sum(s => s.Amount),
                Percentage = totalAmount > 0 
                    ? Math.Round(g.Sum(s => s.Amount) / totalAmount * 100, 2) 
                    : 0
            })
            .OrderByDescending(b => b.TotalAmount)
            .ToList();

        var report = new MonthlyReportDto
        {
            Year = request.Year,
            Month = request.Month,
            MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(request.Month),
            TotalTransactions = slips.Count,
            TotalRevenue = slips.Sum(s => s.Amount),
            SuccessRate = successRate,
            DailyBreakdown = dailyBreakdown,
            BankBreakdown = bankBreakdown
        };

        return report;
    }
}
