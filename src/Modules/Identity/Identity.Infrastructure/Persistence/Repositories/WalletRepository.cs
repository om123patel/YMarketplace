// src/Modules/Identity/Identity.Infrastructure/Persistence/Repositories/WalletRepository.cs

using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;

namespace Identity.Infrastructure.Persistence.Repositories
{
    public class WalletRepository
        : BaseRepository<CustomerWallet, Guid, IdentityDbContext>,
          IWalletRepository
    {
        public WalletRepository(IdentityDbContext db) : base(db) { }

        public async Task<CustomerWallet?> GetByUserIdAsync(
            Guid userId, CancellationToken ct = default)
            => await DbSet
                .FirstOrDefaultAsync(w => w.UserId == userId, ct);

        public async Task<CustomerWallet?> GetByUserIdWithTransactionsAsync(
            Guid userId, int page, int pageSize, CancellationToken ct = default)
        {
            var wallet = await DbSet
                .Include(w => w.Transactions
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize))
                .FirstOrDefaultAsync(w => w.UserId == userId, ct);

            return wallet;
        }

        public async Task<int> CountTransactionsAsync(
            Guid walletId, CancellationToken ct = default)
            => await Context.Set<WalletTransaction>()
                .CountAsync(t => t.WalletId == walletId, ct);
    }
}