using MediatR;
using Microsoft.EntityFrameworkCore;
using SlipVerification.Application.DTOs.Dashboard;
using SlipVerification.Infrastructure.Data;

namespace SlipVerification.Application.Features.Dashboard.Queries;

/// <summary>
/// Query to get recent activities
/// </summary>
public class GetRecentActivitiesQuery : IRequest<List<RecentActivityDto>>
{
    /// <summary>
    /// Number of activities to return (default: 10)
    /// </summary>
    public int Count { get; set; } = 10;
}

/// <summary>
/// Handler for GetRecentActivitiesQuery
/// </summary>
public class GetRecentActivitiesQueryHandler : IRequestHandler<GetRecentActivitiesQuery, List<RecentActivityDto>>
{
    private readonly ApplicationDbContext _context;

    public GetRecentActivitiesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecentActivityDto>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = new List<RecentActivityDto>();

        // Get recent slip verifications
        var recentSlips = await _context.SlipVerifications
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        foreach (var slip in recentSlips)
        {
            var (icon, color, statusText) = GetStatusDisplay(slip.Status.ToString());
            
            activities.Add(new RecentActivityDto
            {
                Id = slip.Id,
                Type = "SlipVerification",
                Description = $"Slip #{slip.ReferenceNumber ?? slip.Id.ToString()[..8]} {statusText}",
                RelatedEntityId = slip.OrderId,
                Status = slip.Status.ToString(),
                Amount = slip.Amount,
                CreatedAt = slip.CreatedAt,
                TimeAgo = GetTimeAgo(slip.CreatedAt),
                Icon = icon,
                Color = color
            });
        }

        // Get recent transactions
        var recentTransactions = await _context.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Take(request.Count / 2)
            .ToListAsync(cancellationToken);

        foreach (var transaction in recentTransactions)
        {
            activities.Add(new RecentActivityDto
            {
                Id = transaction.Id,
                Type = "Transaction",
                Description = $"Transaction {transaction.TransactionType} - {transaction.Amount:C0}",
                RelatedEntityId = transaction.OrderId,
                Status = transaction.TransactionType,
                Amount = transaction.Amount,
                CreatedAt = transaction.CreatedAt,
                TimeAgo = GetTimeAgo(transaction.CreatedAt),
                Icon = "receipt",
                Color = "#2196F3"
            });
        }

        return activities
            .OrderByDescending(a => a.CreatedAt)
            .Take(request.Count)
            .ToList();
    }

    private static (string icon, string color, string statusText) GetStatusDisplay(string status)
    {
        return status switch
        {
            "Verified" => ("check-circle", "#4CAF50", "verified"),
            "Pending" => ("clock-outline", "#FF9800", "pending"),
            "Rejected" => ("close-circle", "#F44336", "rejected"),
            "Processing" => ("sync", "#2196F3", "processing"),
            _ => ("help-circle", "#9E9E9E", status.ToLower())
        };
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "Just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minutes ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hours ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} days ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";
        
        return dateTime.ToString("MMM dd, yyyy");
    }
}
