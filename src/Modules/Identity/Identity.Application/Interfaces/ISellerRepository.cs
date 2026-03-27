using Identity.Application.DTOs.Seller;
using Identity.Domain.Entities;
using Shared.Application.Models;

namespace Identity.Application.Interfaces
{
    public interface ISellerRepository
    {
        Task<PagedList<SellerDto>> GetPagedAsync(int page, int pageSize, string? status,
    string? search, string? sortBy = null, string? sortDirection = null,
    CancellationToken ct = default);

        Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task AddAsync(Seller seller, CancellationToken cancellationToken = default);

        void Update(Seller seller);

        Task<IReadOnlyList<Seller>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
