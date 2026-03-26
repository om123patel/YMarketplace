using Catalog.Application.DTOs.Products;
using Catalog.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Catalog.Application.Interfaces
{
    public interface IProductRepository : IRepository<Product, Guid>
    {
        Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
        Task<PagedList<Product>> GetPagedAsync(ProductFilterRequest filter, CancellationToken ct = default);
        Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
        Task<bool> SlugExistsExceptAsync(string slug, Guid excludeProductId, CancellationToken ct = default);
        Task<bool> SkuExistsAsync(string sku, CancellationToken ct = default);
        Task<bool> SkuExistsExceptAsync(string sku, Guid excludeProductId, CancellationToken ct = default);
        Task<int> CountByStatusAsync(string status, CancellationToken ct = default);
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default);
        Task<IEnumerable<Product>> GetBySellerIdAsync(Guid sellerId, CancellationToken ct = default);
    }
}
