using MediatR;
using Microsoft.EntityFrameworkCore;
using SlipVerification.Application.DTOs.Reports;
using SlipVerification.Infrastructure.Data;

namespace SlipVerification.Application.Features.Reports.Queries;

/// <summary>
/// Query to get daily report
/// </summary>
public class GetDailyReportQuery : IRequest<DailyReportDto>
{
    /// <summary>
    /// Report date
    /// </summary>
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
}

/// <summary>
/// Handler for GetDailyReportQuery
/// </summary>
public class GetDailyReportQueryHandler : IRequestHandler<GetDailyReportQuery, DailyReportDto>
{
    private readonly ApplicationDbContext _context;

    public GetDailyReportQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DailyReportDto> Handle(GetDailyReportQuery request, CancellationToken cancellationToken)
    {
        var date = request.Date.Date;
        var nextDay = date.AddDays(1);

        var slips = await _context.SlipVerifications
            .AsNoTracking()
            .Where(s => s.CreatedAt >= date && s.CreatedAt < nextDay)
            .ToListAsync(cancellationToken);

        var report = new DailyReportDto
        {
            Date = date,
            TotalTransactions = slips.Count,
            TotalRevenue = slips.Sum(s => s.Amount),
            VerifiedCount = slips.Count(s => s.Status == Domain.Enums.SlipVerificationStatus.Verified),
            PendingCount = slips.Count(s => s.Status == Domain.Enums.SlipVerificationStatus.Pending),
            RejectedCount = slips.Count(s => s.Status == Domain.Enums.SlipVerificationStatus.Rejected),
            Transactions = slips.Select(s => new ReportTransactionDto
            {
                Id = s.Id,
                ReferenceNumber = s.ReferenceNumber ?? string.Empty,
                Amount = s.Amount,
                Status = s.Status.ToString(),
                BankName = s.BankName ?? string.Empty,
                TransactionDate = s.TransactionDate ?? s.CreatedAt,
                CreatedAt = s.CreatedAt
            }).OrderByDescending(t => t.CreatedAt).ToList()
        };

        return report;
    }
}
