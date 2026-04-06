using AutoMapper;
using FluentValidation;
using Payments.Application.DTOs.Transactions;
using Payments.Application.Interfaces;
using Payments.Application.Services.Interface;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Payments.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICommissionRuleRepository _commissionRepo;
        private readonly IPaymentsUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateTransactionDto> _createValidator;
        private readonly IValidator<RefundTransactionDto> _refundValidator;

        public TransactionService(
            ITransactionRepository transactionRepo,
            ICommissionRuleRepository commissionRepo,
            IPaymentsUnitOfWork unitOfWork,
            IEventBus eventBus,
            IMapper mapper,
            IValidator<CreateTransactionDto> createValidator,
            IValidator<RefundTransactionDto> refundValidator)
        {
            _transactionRepo = transactionRepo;
            _commissionRepo = commissionRepo;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _mapper = mapper;
            _createValidator = createValidator;
            _refundValidator = refundValidator;
        }

        public async Task<Result<TransactionDto>> GetByIdAsync(
            Guid id, CancellationToken ct = default)
        {
            var tx = await _transactionRepo.GetByIdAsync(id, ct);
            if (tx is null || tx.IsDeleted)
                return Result<TransactionDto>.Failure(
                    $"Transaction {id} not found.", "TRANSACTION_NOT_FOUND");

            return Result<TransactionDto>.Success(_mapper.Map<TransactionDto>(tx));
        }

        public async Task<Result<TransactionDto>> GetByOrderIdAsync(
            Guid orderId, CancellationToken ct = default)
        {
            var tx = await _transactionRepo.GetByOrderIdAsync(orderId, ct);
            if (tx is null)
                return Result<TransactionDto>.Failure(
                    $"No transaction found for order {orderId}.",
                    "TRANSACTION_NOT_FOUND");

            return Result<TransactionDto>.Success(_mapper.Map<TransactionDto>(tx));
        }

        public async Task<Result<PagedList<TransactionListItemDto>>> GetPagedAsync(
            TransactionFilterRequest filter,
            CancellationToken ct = default)
        {
            var paged = await _transactionRepo.GetPagedAsync(filter, ct);
            var mapped = new PagedList<TransactionListItemDto>(
                _mapper.Map<List<TransactionListItemDto>>(paged.Items),
                paged.Page, paged.PageSize, paged.TotalCount);

            return Result<PagedList<TransactionListItemDto>>.Success(mapped);
        }

        public async Task<Result<TransactionDto>> CreateAsync(
            CreateTransactionDto dto,
            CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<TransactionDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Guard: one transaction per order
            if (await _transactionRepo.ExistsByOrderIdAsync(dto.OrderId, ct))
                return Result<TransactionDto>.Failure(
                    $"A transaction already exists for order {dto.OrderId}.",
                    "TRANSACTION_EXISTS");

            // 3. Resolve commission rate
            CommissionRule? rule = dto.CategoryId.HasValue
                ? await _commissionRepo.GetByCategoryIdAsync(dto.CategoryId.Value, ct)
                : null;
            rule ??= await _commissionRepo.GetDefaultAsync(ct);

            var ratePercent = rule?.RatePercent ?? 0m;
            var commissionAmount = Math.Round(dto.Amount * ratePercent / 100m, 2);

            // 4. Parse method
            if (!Enum.TryParse<PaymentMethod>(dto.Method, out var method))
                return Result<TransactionDto>.Failure(
                    $"Invalid payment method '{dto.Method}'.", "VALIDATION_FAILED");

            // 5. Create
            var tx = Transaction.Create(
                orderId: dto.OrderId,
                buyerId: dto.BuyerId,
                sellerId: dto.SellerId,
                storeId: dto.StoreId,
                amount: dto.Amount,
                commissionAmount: commissionAmount,
                currencyCode: dto.CurrencyCode,
                method: method,
                createdBy: dto.BuyerId,
                gatewayProvider: dto.GatewayProvider);

            await _transactionRepo.AddAsync(tx, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<TransactionDto>.Success(_mapper.Map<TransactionDto>(tx));
        }

        public async Task<Result<TransactionDto>> CompleteAsync(
            Guid id, CompleteTransactionDto dto,
            Guid updatedBy, CancellationToken ct = default)
        {
            var tx = await _transactionRepo.GetByIdAsync(id, ct);
            if (tx is null || tx.IsDeleted)
                return Result<TransactionDto>.Failure(
                    $"Transaction {id} not found.", "TRANSACTION_NOT_FOUND");

            try { tx.MarkCompleted(dto.GatewayTransactionId, dto.GatewayResponse, updatedBy); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result<TransactionDto>.Failure(ex.Message, ex.Code); }

            _transactionRepo.Update(tx);
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishEventsAsync(tx, ct);

            return Result<TransactionDto>.Success(_mapper.Map<TransactionDto>(tx));
        }

        public async Task<Result<TransactionDto>> FailAsync(
            Guid id, string reason,
            Guid updatedBy, CancellationToken ct = default)
        {
            var tx = await _transactionRepo.GetByIdAsync(id, ct);
            if (tx is null || tx.IsDeleted)
                return Result<TransactionDto>.Failure(
                    $"Transaction {id} not found.", "TRANSACTION_NOT_FOUND");

            try { tx.MarkFailed(reason, updatedBy); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result<TransactionDto>.Failure(ex.Message, ex.Code); }

            _transactionRepo.Update(tx);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<TransactionDto>.Success(_mapper.Map<TransactionDto>(tx));
        }

        public async Task<Result<TransactionDto>> RefundAsync(
            Guid id, RefundTransactionDto dto,
            Guid adminId, CancellationToken ct = default)
        {
            var validation = await _refundValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<TransactionDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            var tx = await _transactionRepo.GetByIdAsync(id, ct);
            if (tx is null || tx.IsDeleted)
                return Result<TransactionDto>.Failure(
                    $"Transaction {id} not found.", "TRANSACTION_NOT_FOUND");

            try { tx.Refund(dto.Amount, dto.Reason, adminId); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result<TransactionDto>.Failure(ex.Message, ex.Code); }

            _transactionRepo.Update(tx);
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishEventsAsync(tx, ct);

            return Result<TransactionDto>.Success(_mapper.Map<TransactionDto>(tx));
        }

        public async Task<Result<SellerEarningsSummary>> GetSellerEarningsSummaryAsync(
            Guid sellerId, CancellationToken ct = default)
        {
            var totalEarned = await _transactionRepo
                .GetPendingBalanceBySellerIdAsync(sellerId, ct)
                + await _transactionRepo.GetTotalPaidOutBySellerIdAsync(sellerId, ct);

            var totalPaidOut = await _transactionRepo
                .GetTotalPaidOutBySellerIdAsync(sellerId, ct);

            // Commission is not directly stored in a summary; compute from transactions
            var completed = (await _transactionRepo
                .GetCompletedBySellerIdAsync(sellerId, ct)).ToList();

            var totalCommission = completed.Sum(t => t.CommissionAmount);
            var available = completed.Sum(t => t.SellerAmount) - totalPaidOut;

            return Result<SellerEarningsSummary>.Success(new SellerEarningsSummary
            {
                SellerId = sellerId,
                TotalEarned = completed.Sum(t => t.Amount),
                TotalCommission = totalCommission,
                TotalPaidOut = totalPaidOut,
                AvailableBalance = Math.Max(0, available),
                CurrencyCode = "INR"
            });
        }

        private async Task PublishEventsAsync(
            Transaction tx, CancellationToken ct)
        {
            foreach (var e in tx.DomainEvents)
                await _eventBus.PublishAsync(e, ct);
            tx.ClearDomainEvents();
        }
    }
}