using AutoMapper;
using FluentValidation;
using Payments.Application.DTOs.Payouts;
using Payments.Application.Interfaces;
using Payments.Application.Services.Interface;
using Payments.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Payments.Application.Services
{
    public class PayoutService : IPayoutService
    {
        private readonly IPayoutRepository _payoutRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IPaymentsUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;
        private readonly IValidator<RequestPayoutDto> _requestValidator;

        public PayoutService(
            IPayoutRepository payoutRepo,
            ITransactionRepository transactionRepo,
            IPaymentsUnitOfWork unitOfWork,
            IEventBus eventBus,
            IMapper mapper,
            IValidator<RequestPayoutDto> requestValidator)
        {
            _payoutRepo = payoutRepo;
            _transactionRepo = transactionRepo;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _mapper = mapper;
            _requestValidator = requestValidator;
        }

        public async Task<Result<PayoutDto>> GetByIdAsync(
            Guid id, CancellationToken ct = default)
        {
            var payout = await _payoutRepo.GetByIdAsync(id, ct);
            if (payout is null || payout.IsDeleted)
                return Result<PayoutDto>.Failure(
                    $"Payout {id} not found.", "PAYOUT_NOT_FOUND");

            return Result<PayoutDto>.Success(_mapper.Map<PayoutDto>(payout));
        }

        public async Task<Result<PagedList<PayoutListItemDto>>> GetPagedAsync(
            PayoutFilterRequest filter,
            CancellationToken ct = default)
        {
            var paged = await _payoutRepo.GetPagedAsync(filter, ct);
            var mapped = new PagedList<PayoutListItemDto>(
                _mapper.Map<List<PayoutListItemDto>>(paged.Items),
                paged.Page, paged.PageSize, paged.TotalCount);

            return Result<PagedList<PayoutListItemDto>>.Success(mapped);
        }

        public async Task<Result<IEnumerable<PayoutListItemDto>>> GetBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default)
        {
            var payouts = await _payoutRepo.GetBySellerIdAsync(sellerId, ct);
            return Result<IEnumerable<PayoutListItemDto>>.Success(
                _mapper.Map<IEnumerable<PayoutListItemDto>>(payouts));
        }

        public async Task<Result<PayoutDto>> RequestAsync(
            RequestPayoutDto dto, Guid sellerId,
            CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _requestValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<PayoutDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Guard: no duplicate pending payout
            if (await _payoutRepo.HasPendingPayoutAsync(sellerId, ct))
                return Result<PayoutDto>.Failure(
                    "You already have a pending payout request.",
                    "PENDING_PAYOUT_EXISTS");

            // 3. Check available balance
            var available = await _transactionRepo
                .GetPendingBalanceBySellerIdAsync(sellerId, ct);

            if (dto.Amount > available)
                return Result<PayoutDto>.Failure(
                    $"Requested amount ({dto.Amount}) exceeds available balance ({available}).",
                    "INSUFFICIENT_BALANCE");

            // 4. Create
            var payout = Payout.Create(
                sellerId: sellerId,
                amount: dto.Amount,
                currencyCode: dto.CurrencyCode,
                createdBy: sellerId,
                bankAccountNumber: dto.BankAccountNumber,
                bankIfscCode: dto.BankIfscCode,
                bankAccountName: dto.BankAccountName,
                upiId: dto.UpiId);

            await _payoutRepo.AddAsync(payout, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<PayoutDto>.Success(_mapper.Map<PayoutDto>(payout));
        }

        public async Task<Result<PayoutDto>> StartProcessingAsync(
            Guid id, string? note,
            Guid adminId, CancellationToken ct = default)
        {
            var payout = await _payoutRepo.GetByIdAsync(id, ct);
            if (payout is null || payout.IsDeleted)
                return Result<PayoutDto>.Failure(
                    $"Payout {id} not found.", "PAYOUT_NOT_FOUND");

            try { payout.StartProcessing(adminId, note); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result<PayoutDto>.Failure(ex.Message, ex.Code); }

            _payoutRepo.Update(payout);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<PayoutDto>.Success(_mapper.Map<PayoutDto>(payout));
        }

        public async Task<Result<PayoutDto>> CompleteAsync(
            Guid id, ProcessPayoutDto dto,
            Guid adminId, CancellationToken ct = default)
        {
            var payout = await _payoutRepo.GetByIdAsync(id, ct);
            if (payout is null || payout.IsDeleted)
                return Result<PayoutDto>.Failure(
                    $"Payout {id} not found.", "PAYOUT_NOT_FOUND");

            try { payout.MarkCompleted(dto.GatewayReference, adminId); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result<PayoutDto>.Failure(ex.Message, ex.Code); }

            _payoutRepo.Update(payout);
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishEventsAsync(payout, ct);

            return Result<PayoutDto>.Success(_mapper.Map<PayoutDto>(payout));
        }

        public async Task<Result<PayoutDto>> FailAsync(
            Guid id, string reason,
            Guid adminId, CancellationToken ct = default)
        {
            var payout = await _payoutRepo.GetByIdAsync(id, ct);
            if (payout is null || payout.IsDeleted)
                return Result<PayoutDto>.Failure(
                    $"Payout {id} not found.", "PAYOUT_NOT_FOUND");

            try { payout.MarkFailed(reason, adminId); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result<PayoutDto>.Failure(ex.Message, ex.Code); }

            _payoutRepo.Update(payout);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<PayoutDto>.Success(_mapper.Map<PayoutDto>(payout));
        }

        public async Task<Result<PayoutDto>> CancelAsync(
            Guid id, string reason,
            Guid cancelledBy, CancellationToken ct = default)
        {
            var payout = await _payoutRepo.GetByIdAsync(id, ct);
            if (payout is null || payout.IsDeleted)
                return Result<PayoutDto>.Failure(
                    $"Payout {id} not found.", "PAYOUT_NOT_FOUND");

            try { payout.Cancel(reason, cancelledBy); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result<PayoutDto>.Failure(ex.Message, ex.Code); }

            _payoutRepo.Update(payout);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<PayoutDto>.Success(_mapper.Map<PayoutDto>(payout));
        }

        private async Task PublishEventsAsync(
            Payout payout, CancellationToken ct)
        {
            foreach (var e in payout.DomainEvents)
                await _eventBus.PublishAsync(e, ct);
            payout.ClearDomainEvents();
        }
    }
}