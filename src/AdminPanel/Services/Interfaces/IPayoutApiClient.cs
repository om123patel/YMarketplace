using Payments.Application.DTOs.Payouts;
using Shared.Application.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface IPayoutApiClient
    {
        Task<PagedList<PayoutListItemDto>?> GetPagedAsync(
            PayoutFilterRequest filter);

        Task<PayoutDto?> GetByIdAsync(Guid id);

        Task<PayoutDto?> StartProcessingAsync(Guid id, string? note);

        Task<PayoutDto?> CompleteAsync(Guid id, ProcessPayoutDto dto);

        Task<PayoutDto?> FailAsync(Guid id, string reason);

        Task<PayoutDto?> CancelAsync(Guid id, string reason);
    }
}
