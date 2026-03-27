using Identity.Application.DTOs.Seller;
using Shared.Application.Models;

namespace Identity.Application.Services.Interfaces
{
    public interface ISellerService
    {
        Task<Result<CreateSellerDto>> CreateAsync(CreateSellerDto command, CancellationToken ct = default);

        Task<Result<SellerDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<Result<PagedList<SellerDto>>> GetPagedAsync(
            int page, int pageSize,
            string? status, string? search,
            string? sortBy = null, string? sortDirection = null,
            CancellationToken ct = default);

        Task<Result> ApproveAsync(Guid id, Guid adminId, CancellationToken ct = default);

        Task<Result> RejectAsync(Guid id, Guid adminId, string reason, CancellationToken ct = default);

        Task<Result> SuspendAsync(Guid id, CancellationToken ct = default);

        Task<Result> ActivateAsync(Guid id, CancellationToken ct = default);

        Task<Result> UpdateAsync(UpdateSellerDto command, CancellationToken ct = default);
    }
}
