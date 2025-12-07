using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlipVerification.Application.DTOs.Reports;
using SlipVerification.Application.Features.Reports.Queries;

namespace SlipVerification.API.Controllers.v1;

/// <summary>
/// Reports endpoints for generating and exporting reports
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IMediator mediator, ILogger<ReportsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get daily report for a specific date
    /// </summary>
    /// <param name="date">Report date (defaults to today)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Daily report</returns>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(DailyReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDailyReport(
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        var reportDate = date ?? DateTime.UtcNow.Date;
        _logger.LogInformation("Generating daily report for {Date}", reportDate);
        
        var query = new GetDailyReportQuery { Date = reportDate };
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Get monthly report for a specific year and month
    /// </summary>
    /// <param name="year">Report year</param>
    /// <param name="month">Report month (1-12)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Monthly report</returns>
    [HttpGet("monthly")]
    [ProducesResponseType(typeof(MonthlyReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMonthlyReport(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var reportYear = year ?? DateTime.UtcNow.Year;
        var reportMonth = month ?? DateTime.UtcNow.Month;

        if (reportMonth < 1 || reportMonth > 12)
        {
            return BadRequest(new { message = "Month must be between 1 and 12" });
        }

        _logger.LogInformation("Generating monthly report for {Year}/{Month}", reportYear, reportMonth);
        
        var query = new GetMonthlyReportQuery { Year = reportYear, Month = reportMonth };
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Generate custom report based on filters
    /// </summary>
    /// <param name="request">Custom report request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Custom report</returns>
    [HttpPost("custom")]
    [ProducesResponseType(typeof(DailyReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCustomReport(
        [FromBody] CustomReportRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }

        _logger.LogInformation("Generating custom report from {StartDate} to {EndDate}", 
            request.StartDate, request.EndDate);
        
        // For now, return a daily report for the start date
        // Can be enhanced to support full date range filtering
        var query = new GetDailyReportQuery { Date = request.StartDate };
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Export report to file (Excel/CSV)
    /// </summary>
    /// <param name="options">Export options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File download</returns>
    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportReport(
        [FromBody] ExportOptionsDto options,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Exporting report in {Format} format from {StartDate} to {EndDate}", 
            options.Format, options.StartDate, options.EndDate);

        // Generate CSV content
        var csvContent = await GenerateCsvReport(options, cancellationToken);
        
        var fileName = $"report_{options.StartDate:yyyyMMdd}_{options.EndDate:yyyyMMdd}.csv";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        
        return File(bytes, "text/csv", fileName);
    }

    /// <summary>
    /// Get available report types and options
    /// </summary>
    /// <returns>Report options</returns>
    [HttpGet("options")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetReportOptions()
    {
        var options = new
        {
            reportTypes = new[] { "daily", "monthly", "custom" },
            exportFormats = new[] { "csv", "excel", "pdf" },
            availableColumns = new[] 
            { 
                "id", "referenceNumber", "amount", "status", 
                "bankName", "transactionDate", "createdAt" 
            },
            statusFilters = new[] { "All", "Verified", "Pending", "Rejected" }
        };

        return Ok(options);
    }

    private async Task<string> GenerateCsvReport(ExportOptionsDto options, CancellationToken cancellationToken)
    {
        var query = new GetDailyReportQuery { Date = options.StartDate };
        var report = await _mediator.Send(query, cancellationToken);

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("ID,Reference Number,Amount,Status,Bank Name,Transaction Date,Created At");

        foreach (var transaction in report.Transactions)
        {
            csv.AppendLine($"{transaction.Id},{transaction.ReferenceNumber},{transaction.Amount}," +
                          $"{transaction.Status},{transaction.BankName}," +
                          $"{transaction.TransactionDate:yyyy-MM-dd HH:mm:ss}," +
                          $"{transaction.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return csv.ToString();
    }
}
