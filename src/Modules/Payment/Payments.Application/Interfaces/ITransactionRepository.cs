using Payments.Application.DTOs.Transactions;
using Payments.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Payments.Application.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction, Guid>
    {
        Task<PagedList<Transaction>> GetPagedAsync(
            TransactionFilterRequest filter,
            CancellationToken ct = default);

        Task<Transaction?> GetByOrderIdAsync(
            Guid orderId, CancellationToken ct = default);

        Task<IEnumerable<Transaction>> GetCompletedBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default);

        Task<decimal> GetTotalPaidOutBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default);

        Task<decimal> GetPendingBalanceBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default);

        Task<bool> ExistsByOrderIdAsync(
            Guid orderId, CancellationToken ct = default);
    }
}