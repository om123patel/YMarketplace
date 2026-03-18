using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Shared.Application.Models;

namespace Identity.Application.Interfaces
{
    public interface IVendorApplicationRepository
    {
        Task<VendorApplication?> GetByIdAsync(
            Guid id, CancellationToken ct = default);
        Task<VendorApplication?> GetByUserIdAsync(
            Guid userId, CancellationToken ct = default);
        Task<PagedList<VendorApplication>> GetPagedAsync(
            int page, int pageSize,
            VendorApplicationStatus? status,
            CancellationToken ct = default);
        Task AddAsync(VendorApplication app, CancellationToken ct = default);
        void Update(VendorApplication app);
    }

}
