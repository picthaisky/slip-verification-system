using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlipVerification.Application.DTOs.Slips;
using SlipVerification.Application.Features.Slips.Commands;
using SlipVerification.Application.Features.Slips.Queries;

namespace SlipVerification.API.Controllers.v1;

/// <summary>
/// Slip verification endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class SlipsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SlipsController> _logger;

    public SlipsController(IMediator mediator, ILogger<SlipsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Upload and verify a payment slip
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="file">Slip image file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Slip verification details</returns>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(SlipVerificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifySlip(
        [FromForm] Guid orderId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var command = new VerifySlipCommand
        {
            OrderId = orderId,
            ImageData = memoryStream.ToArray(),
            ImageFileName = file.FileName,
            ImageContentType = file.ContentType
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get slip by ID
    /// </summary>
    /// <param name="id">Slip ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Slip details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SlipVerificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSlipById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSlipByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get slips by order ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of slips for the order</returns>
    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(IEnumerable<SlipVerificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSlipsByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        // This would need a corresponding query handler
        return Ok(new List<SlipVerificationDto>());
    }

    /// <summary>
    /// Delete a slip (soft delete)
    /// </summary>
    /// <param name="id">Slip ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteSlip(Guid id, CancellationToken cancellationToken)
    {
        // This would need a corresponding command handler
        return NoContent();
    }
}
