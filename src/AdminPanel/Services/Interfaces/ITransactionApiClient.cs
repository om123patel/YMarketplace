using Payments.Application.DTOs.Transactions;
using Shared.Application.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface ITransactionApiClient
    {
        Task<PagedList<TransactionListItemDto>?> GetPagedAsync(
            TransactionFilterRequest filter);

        Task<TransactionDto?> GetByIdAsync(Guid id);

        Task<TransactionDto?> GetByOrderIdAsync(Guid orderId);

        Task<TransactionDto?> RefundAsync(Guid id, RefundTransactionDto dto);
    }
}