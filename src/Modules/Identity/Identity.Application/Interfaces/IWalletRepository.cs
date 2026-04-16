// src/Modules/Identity/Identity.Application/Interfaces/IWalletRepository.cs

using Identity.Domain.Entities;
using Shared.Application.Interfaces;

namespace Identity.Application.Interfaces
{
    public interface IWalletRepository : IRepository<CustomerWallet, Guid>
    {
        Task<CustomerWallet?> GetByUserIdAsync(
            Guid userId, CancellationToken ct = default);
        Task<CustomerWallet?> GetByUserIdWithTransactionsAsync(
            Guid userId, int page, int pageSize, CancellationToken ct = default);
        Task<int> CountTransactionsAsync(
            Guid walletId, CancellationToken ct = default);
    }
}