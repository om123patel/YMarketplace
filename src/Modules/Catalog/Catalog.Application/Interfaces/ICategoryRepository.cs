using Catalog.Application.DTOs.Categories;
using Catalog.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Catalog.Application.Interfaces
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        Task<PagedList<Category>> GetPagedAsync(CategoryFilterRequest filter, CancellationToken ct = default);
        Task<IEnumerable<Category>> GetAllActiveAsync(CancellationToken ct = default);
        Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken ct = default);
        Task<IEnumerable<Category>> GetChildrenAsync(int parentId, CancellationToken ct = default);
        Task<Category?> GetByIdWithChildrenAsync(int id, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
        Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
        Task<bool> SlugExistsExceptAsync(string slug, int excludeId, CancellationToken ct = default);
        Task<bool> HasChildrenAsync(int id, CancellationToken ct = default);
        Task<bool> HasProductsAsync(int id, CancellationToken ct = default);
    }
}
