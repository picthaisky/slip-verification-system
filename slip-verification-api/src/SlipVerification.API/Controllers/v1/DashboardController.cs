using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlipVerification.Application.DTOs.Dashboard;
using SlipVerification.Application.Features.Dashboard.Queries;

namespace SlipVerification.API.Controllers.v1;

/// <summary>
/// Dashboard endpoints for statistics and recent activities
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics overview
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dashboard statistics");
        
        var query = new GetDashboardStatsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Get recent activities for dashboard
    /// </summary>
    /// <param name="count">Number of activities to return (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recent activities</returns>
    [HttpGet("recent-activities")]
    [ProducesResponseType(typeof(List<RecentActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecentActivities(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching {Count} recent activities", count);
        
        var query = new GetRecentActivitiesQuery { Count = count };
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Get chart data for dashboard visualizations
    /// </summary>
    /// <param name="period">Period type (daily, weekly, monthly)</param>
    /// <param name="count">Number of periods to include (default: 7)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chart data for visualizations</returns>
    [HttpGet("chart-data")]
    [ProducesResponseType(typeof(ChartDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetChartData(
        [FromQuery] string period = "daily",
        [FromQuery] int count = 7,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching chart data for period: {Period}, count: {Count}", period, count);
        
        var query = new GetChartDataQuery { Period = period, Count = count };
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Get transaction summary by bank
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction summary grouped by bank</returns>
    [HttpGet("bank-summary")]
    [ProducesResponseType(typeof(List<TransactionSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBankSummary(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching bank summary");
        
        // This could be enhanced with a specific query handler
        // For now, return empty list
        return Ok(new List<TransactionSummaryDto>());
    }
}
