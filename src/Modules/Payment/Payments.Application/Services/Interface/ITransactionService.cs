using Payments.Application.DTOs.Transactions;
using Shared.Application.Models;

namespace Payments.Application.Services.Interface
{
    public interface ITransactionService
    {
        Task<Result<TransactionDto>> GetByIdAsync(
            Guid id, CancellationToken ct = default);

        Task<Result<TransactionDto>> GetByOrderIdAsync(
            Guid orderId, CancellationToken ct = default);

        Task<Result<PagedList<TransactionListItemDto>>> GetPagedAsync(
            TransactionFilterRequest filter,
            CancellationToken ct = default);

        Task<Result<TransactionDto>> CreateAsync(
            CreateTransactionDto dto,
            CancellationToken ct = default);

        Task<Result<TransactionDto>> CompleteAsync(
            Guid id, CompleteTransactionDto dto,
            Guid updatedBy, CancellationToken ct = default);

        Task<Result<TransactionDto>> FailAsync(
            Guid id, string reason,
            Guid updatedBy, CancellationToken ct = default);

        Task<Result<TransactionDto>> RefundAsync(
            Guid id, RefundTransactionDto dto,
            Guid adminId, CancellationToken ct = default);

        Task<Result<SellerEarningsSummary>> GetSellerEarningsSummaryAsync(
            Guid sellerId, CancellationToken ct = default);
    }
}