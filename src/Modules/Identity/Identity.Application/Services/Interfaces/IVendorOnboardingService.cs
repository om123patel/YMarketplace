using Identity.Application.DTOs;
using Shared.Application.Models;

namespace Identity.Application.Services.Interfaces
{
    public interface IVendorOnboardingService
    {
        Task<Result<VendorApplicationDto>> ApplyAsync(
            Guid userId, ApplyVendorDto dto, CancellationToken ct = default);
        Task<Result<PagedList<VendorApplicationDto>>> GetPendingAsync(
            int page, int pageSize, CancellationToken ct = default);
        Task<Result<VendorApplicationDto>> GetByIdAsync(
            Guid id, CancellationToken ct = default);
        Task<Result> ApproveAsync(
            Guid applicationId, Guid adminId, CancellationToken ct = default);
        Task<Result> RejectAsync(
            Guid applicationId, Guid adminId,
            string reason, CancellationToken ct = default);
    }
}
