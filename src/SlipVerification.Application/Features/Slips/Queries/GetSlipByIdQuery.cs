using MediatR;
using SlipVerification.Application.DTOs.Slips;
using SlipVerification.Shared.Results;

namespace SlipVerification.Application.Features.Slips.Queries;

/// <summary>
/// Query to get a slip by ID
/// </summary>
public class GetSlipByIdQuery : IRequest<Result<SlipVerificationDto>>
{
    public Guid Id { get; set; }
}
