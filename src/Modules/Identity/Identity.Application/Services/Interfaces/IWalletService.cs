// src/Modules/Identity/Identity.Application/Services/Interfaces/IWalletService.cs

using Identity.Application.DTOs.Wallet;
using Shared.Application.Models;

namespace Identity.Application.Services.Interfaces
{
    public interface IWalletService
    {
        Task<Result<WalletDto>> GetBalanceAsync(
            Guid userId, CancellationToken ct = default);
        Task<Result<WalletHistoryDto>> GetHistoryAsync(
            Guid userId, int page, int pageSize, CancellationToken ct = default);
        Task<Result<WalletDto>> AddMoneyAsync(
            Guid userId, decimal amount, string? referenceId,
            CancellationToken ct = default);
    }
}