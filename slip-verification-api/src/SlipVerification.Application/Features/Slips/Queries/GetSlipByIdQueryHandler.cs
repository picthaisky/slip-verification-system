using MediatR;
using SlipVerification.Application.DTOs.Slips;
using SlipVerification.Domain.Interfaces;
using SlipVerification.Shared.Results;

namespace SlipVerification.Application.Features.Slips.Queries;

/// <summary>
/// Handler for GetSlipByIdQuery
/// </summary>
public class GetSlipByIdQueryHandler : IRequestHandler<GetSlipByIdQuery, Result<SlipVerificationDto>>
{
    private readonly IRepository<Domain.Entities.SlipVerification> _slipRepository;

    public GetSlipByIdQueryHandler(IRepository<Domain.Entities.SlipVerification> slipRepository)
    {
        _slipRepository = slipRepository;
    }

    public async Task<Result<SlipVerificationDto>> Handle(GetSlipByIdQuery request, CancellationToken cancellationToken)
    {
        var slip = await _slipRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (slip == null)
        {
            return Result<SlipVerificationDto>.Failure("Slip not found");
        }

        var dto = new SlipVerificationDto
        {
            Id = slip.Id,
            OrderId = slip.OrderId,
            UserId = slip.UserId,
            ImagePath = slip.ImagePath,
            ImageHash = slip.ImageHash,
            Amount = slip.Amount,
            TransactionDate = slip.TransactionDate,
            TransactionTime = slip.TransactionTime,
            ReferenceNumber = slip.ReferenceNumber,
            BankName = slip.BankName,
            BankAccountNumber = slip.BankAccountNumber,
            Status = slip.Status.ToString(),
            RawOcrText = slip.RawOcrText,
            OcrConfidence = slip.OcrConfidence,
            VerificationNotes = slip.VerificationNotes,
            VerifiedBy = slip.VerifiedBy,
            VerifiedAt = slip.VerifiedAt,
            CreatedAt = slip.CreatedAt
        };

        return Result<SlipVerificationDto>.Success(dto);
    }
}
