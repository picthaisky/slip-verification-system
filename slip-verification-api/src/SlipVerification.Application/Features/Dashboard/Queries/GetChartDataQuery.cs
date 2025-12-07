using MediatR;
using Microsoft.EntityFrameworkCore;
using SlipVerification.Application.DTOs.Dashboard;
using SlipVerification.Infrastructure.Data;

namespace SlipVerification.Application.Features.Dashboard.Queries;

/// <summary>
/// Query to get chart data for dashboard
/// </summary>
public class GetChartDataQuery : IRequest<ChartDataDto>
{
    /// <summary>
    /// Period type (daily, weekly, monthly)
    /// </summary>
    public string Period { get; set; } = "daily";

    /// <summary>
    /// Number of periods to include
    /// </summary>
    public int Count { get; set; } = 7;
}

/// <summary>
/// Handler for GetChartDataQuery
/// </summary>
public class GetChartDataQueryHandler : IRequestHandler<GetChartDataQuery, ChartDataDto>
{
    private readonly ApplicationDbContext _context;

    public GetChartDataQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChartDataDto> Handle(GetChartDataQuery request, CancellationToken cancellationToken)
    {
        var chartData = new ChartDataDto();
        var today = DateTime.UtcNow.Date;

        var slipVerifications = await _context.SlipVerifications
            .AsNoTracking()
            .Where(s => s.CreatedAt >= today.AddDays(-request.Count))
            .ToListAsync(cancellationToken);

        var transactionCounts = new List<decimal>();
        var verifiedCounts = new List<decimal>();
        var revenueData = new List<decimal>();

        for (int i = request.Count - 1; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var daySlips = slipVerifications.Where(s => s.CreatedAt.Date == date).ToList();

            chartData.Labels.Add(date.ToString("MMM dd"));
            transactionCounts.Add(daySlips.Count);
            verifiedCounts.Add(daySlips.Count(s => s.Status == Domain.Enums.SlipVerificationStatus.Verified));
            revenueData.Add(daySlips.Sum(s => s.Amount));
        }

        chartData.Datasets.Add(new ChartDatasetDto
        {
            Label = "Total Transactions",
            Data = transactionCounts,
            BackgroundColor = "rgba(33, 150, 243, 0.2)",
            BorderColor = "rgba(33, 150, 243, 1)"
        });

        chartData.Datasets.Add(new ChartDatasetDto
        {
            Label = "Verified",
            Data = verifiedCounts,
            BackgroundColor = "rgba(76, 175, 80, 0.2)",
            BorderColor = "rgba(76, 175, 80, 1)"
        });

        chartData.Datasets.Add(new ChartDatasetDto
        {
            Label = "Revenue",
            Data = revenueData,
            BackgroundColor = "rgba(156, 39, 176, 0.2)",
            BorderColor = "rgba(156, 39, 176, 1)"
        });

        return chartData;
    }
}
