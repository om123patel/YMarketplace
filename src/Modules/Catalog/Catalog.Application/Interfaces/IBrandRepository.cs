using Catalog.Application.DTOs.Brands;
using Catalog.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Catalog.Application.Interfaces
{
    public interface IBrandRepository : IRepository<Brand, int>
    {
        Task<PagedList<Brand>> GetPagedAsync(BrandFilterRequest filter, CancellationToken ct = default);
        Task<IEnumerable<Brand>> GetAllActiveAsync(CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
        Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
        Task<bool> SlugExistsExceptAsync(string slug, int excludeId, CancellationToken ct = default);
        Task<bool> NameExistsAsync(string name, CancellationToken ct = default);
        Task<bool> NameExistsExceptAsync(string name, int excludeId, CancellationToken ct = default);
        Task<bool> HasProductsAsync(int id, CancellationToken ct = default);
    }
}
