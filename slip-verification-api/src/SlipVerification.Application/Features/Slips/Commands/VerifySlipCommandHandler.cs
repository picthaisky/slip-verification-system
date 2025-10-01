using MediatR;
using Microsoft.Extensions.Logging;
using SlipVerification.Application.DTOs.Slips;
using SlipVerification.Application.Interfaces;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;
using SlipVerification.Domain.Interfaces;
using SlipVerification.Shared.Results;

namespace SlipVerification.Application.Features.Slips.Commands;

/// <summary>
/// Handler for VerifySlipCommand
/// </summary>
public class VerifySlipCommandHandler : IRequestHandler<VerifySlipCommand, Result<SlipVerificationDto>>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Domain.Entities.SlipVerification> _slipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<VerifySlipCommandHandler> _logger;

    public VerifySlipCommandHandler(
        IRepository<Order> orderRepository,
        IRepository<Domain.Entities.SlipVerification> slipRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<VerifySlipCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _slipRepository = slipRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<SlipVerificationDto>> Handle(VerifySlipCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate order exists
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return Result<SlipVerificationDto>.Failure("Order not found");
            }

            // Upload slip image
            var imagePath = await _fileStorageService.SaveFileAsync(
                request.ImageData, 
                request.ImageFileName, 
                "slips", 
                cancellationToken);

            // Create slip verification record
            var slip = new Domain.Entities.SlipVerification
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                ImagePath = imagePath,
                Status = VerificationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _slipRepository.AddAsync(slip, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Slip verification created for order {OrderId}", request.OrderId);

            // Map to DTO
            var dto = new SlipVerificationDto
            {
                Id = slip.Id,
                OrderId = slip.OrderId,
                ImagePath = slip.ImagePath,
                Amount = slip.Amount,
                TransactionDate = slip.TransactionDate,
                Status = slip.Status.ToString(),
                CreatedAt = slip.CreatedAt
            };

            return Result<SlipVerificationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating slip verification for order {OrderId}", request.OrderId);
            return Result<SlipVerificationDto>.Failure("An error occurred while processing the slip");
        }
    }
}
