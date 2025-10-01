using MediatR;
using SlipVerification.Application.DTOs.Slips;
using SlipVerification.Shared.Results;

namespace SlipVerification.Application.Features.Slips.Commands;

/// <summary>
/// Command to verify a payment slip
/// </summary>
public class VerifySlipCommand : IRequest<Result<SlipVerificationDto>>
{
    public Guid OrderId { get; set; }
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string ImageFileName { get; set; } = string.Empty;
    public string ImageContentType { get; set; } = string.Empty;
}
