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
            ImagePath = slip.ImagePath,
            Amount = slip.Amount,
            TransactionDate = slip.TransactionDate,
            ReferenceNumber = slip.ReferenceNumber,
            BankName = slip.BankName,
            SenderAccount = slip.SenderAccount,
            ReceiverAccount = slip.ReceiverAccount,
            Status = slip.Status.ToString(),
            RawOcrText = slip.RawOcrText,
            OcrConfidence = slip.OcrConfidence,
            VerificationNotes = slip.VerificationNotes,
            VerifiedAt = slip.VerifiedAt,
            CreatedAt = slip.CreatedAt
        };

        return Result<SlipVerificationDto>.Success(dto);
    }
}
