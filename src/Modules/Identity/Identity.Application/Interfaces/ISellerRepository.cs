using Identity.Application.DTOs.Seller;
using Identity.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Identity.Application.Interfaces
{
    public interface ISellerRepository : IRepository<Seller, Guid>
    {
        Task<PagedList<SellerDto>> GetPagedAsync(int page, int pageSize, string? status,
        string? search, string? sortBy = null, string? sortDirection = null,
        CancellationToken ct = default);

        Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

      
    }
}
