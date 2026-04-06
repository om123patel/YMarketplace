using Payments.Application.DTOs.Payouts;
using Shared.Application.Models;

namespace Payments.Application.Services.Interface
{
    public interface IPayoutService
    {
        Task<Result<PayoutDto>> GetByIdAsync(
            Guid id, CancellationToken ct = default);

        Task<Result<PagedList<PayoutListItemDto>>> GetPagedAsync(
            PayoutFilterRequest filter,
            CancellationToken ct = default);

        Task<Result<IEnumerable<PayoutListItemDto>>> GetBySellerIdAsync(
            Guid sellerId, CancellationToken ct = default);

        Task<Result<PayoutDto>> RequestAsync(
            RequestPayoutDto dto, Guid sellerId,
            CancellationToken ct = default);

        Task<Result<PayoutDto>> StartProcessingAsync(
            Guid id, string? note,
            Guid adminId, CancellationToken ct = default);

        Task<Result<PayoutDto>> CompleteAsync(
            Guid id, ProcessPayoutDto dto,
            Guid adminId, CancellationToken ct = default);

        Task<Result<PayoutDto>> FailAsync(
            Guid id, string reason,
            Guid adminId, CancellationToken ct = default);

        Task<Result<PayoutDto>> CancelAsync(
            Guid id, string reason,
            Guid cancelledBy, CancellationToken ct = default);
    }
}