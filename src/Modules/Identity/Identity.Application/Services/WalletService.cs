// src/Modules/Identity/Identity.Application/Services/WalletService.cs

using Identity.Application.DTOs.Wallet;
using Identity.Application.Interfaces;
using Identity.Application.Services.Interfaces;
using Identity.Domain.Entities;
using Shared.Application.Models;

namespace Identity.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepo;
        private readonly IIdentityUnitOfWork _unitOfWork;

        public WalletService(
            IWalletRepository walletRepo,
            IIdentityUnitOfWork unitOfWork)
        {
            _walletRepo = walletRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<WalletDto>> GetBalanceAsync(
            Guid userId, CancellationToken ct = default)
        {
            var wallet = await GetOrCreateWalletAsync(userId, ct);
            return Result<WalletDto>.Success(MapWallet(wallet));
        }

        public async Task<Result<WalletHistoryDto>> GetHistoryAsync(
            Guid userId, int page, int pageSize, CancellationToken ct = default)
        {
            var wallet = await _walletRepo
                .GetByUserIdWithTransactionsAsync(userId, page, pageSize, ct);

            if (wallet is null)
            {
                wallet = CustomerWallet.Create(userId, userId);
                await _walletRepo.AddAsync(wallet, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            var totalCount = await _walletRepo.CountTransactionsAsync(wallet.Id, ct);

            var txns = wallet.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Type = t.Type.ToString(),
                    Description = t.Description,
                    ReferenceId = t.ReferenceId,
                    BalanceAfter = t.BalanceAfter,
                    CreatedAt = t.CreatedAt
                }).ToList();

            return Result<WalletHistoryDto>.Success(new WalletHistoryDto
            {
                Wallet = MapWallet(wallet),
                Transactions = txns,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<Result<WalletDto>> AddMoneyAsync(
            Guid userId, decimal amount, string? referenceId,
            CancellationToken ct = default)
        {
            if (amount <= 0)
                return Result<WalletDto>.Failure(
                    "Amount must be greater than zero.", "VALIDATION_FAILED");

            var wallet = await GetOrCreateWalletAsync(userId, ct);

            try
            {
                wallet.Credit(amount, "Wallet top-up", referenceId, userId);
            }
            catch (Shared.Domain.Exceptions.DomainException ex)
            {
                return Result<WalletDto>.Failure(ex.Message, ex.Code);
            }

            _walletRepo.Update(wallet);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<WalletDto>.Success(MapWallet(wallet));
        }

        // ── Helpers ───────────────────────────────────────────────
        private async Task<CustomerWallet> GetOrCreateWalletAsync(
            Guid userId, CancellationToken ct)
        {
            var wallet = await _walletRepo.GetByUserIdAsync(userId, ct);
            if (wallet is not null) return wallet;

            wallet = CustomerWallet.Create(userId, userId);
            await _walletRepo.AddAsync(wallet, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return wallet;
        }

        private static WalletDto MapWallet(CustomerWallet w) => new()
        {
            Id = w.Id,
            UserId = w.UserId,
            Balance = w.Balance,
            CurrencyCode = w.CurrencyCode,
            IsActive = w.IsActive
        };
    }
}