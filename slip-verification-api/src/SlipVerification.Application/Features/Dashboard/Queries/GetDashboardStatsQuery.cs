using MediatR;
using Microsoft.EntityFrameworkCore;
using SlipVerification.Application.DTOs.Dashboard;
using SlipVerification.Infrastructure.Data;

namespace SlipVerification.Application.Features.Dashboard.Queries;

/// <summary>
/// Query to get dashboard statistics
/// </summary>
public class GetDashboardStatsQuery : IRequest<DashboardStatsDto>
{
}

/// <summary>
/// Handler for GetDashboardStatsQuery
/// </summary>
public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly ApplicationDbContext _context;

    public GetDashboardStatsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        
        // Get slip verifications stats
        var slipVerifications = await _context.SlipVerifications
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalTransactions = slipVerifications.Count;
        var verifiedCount = slipVerifications.Count(s => s.Status == Domain.Enums.SlipVerificationStatus.Verified);
        var pendingCount = slipVerifications.Count(s => s.Status == Domain.Enums.SlipVerificationStatus.Pending);
        var rejectedCount = slipVerifications.Count(s => s.Status == Domain.Enums.SlipVerificationStatus.Rejected);

        var todaySlips = slipVerifications.Where(s => s.CreatedAt.Date == today).ToList();
        var todayTransactions = todaySlips.Count;
        var todayRevenue = todaySlips.Sum(s => s.Amount);

        var totalRevenue = slipVerifications.Sum(s => s.Amount);
        var successRate = totalTransactions > 0 
            ? Math.Round((decimal)verifiedCount / totalTransactions * 100, 2) 
            : 0;

        // Calculate average processing time (mock for now)
        var averageProcessingTime = 2.5; // seconds

        return new DashboardStatsDto
        {
            TotalTransactions = totalTransactions,
            TotalRevenue = totalRevenue,
            VerifiedCount = verifiedCount,
            PendingCount = pendingCount,
            RejectedCount = rejectedCount,
            SuccessRate = successRate,
            AverageProcessingTime = averageProcessingTime,
            TodayTransactions = todayTransactions,
            TodayRevenue = todayRevenue
        };
    }
}
