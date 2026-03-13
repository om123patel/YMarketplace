using Catalog.Domain.Entities;
using Shared.Application.Interfaces;

namespace Catalog.Application.Interfaces
{
    public interface IAttributeTemplateRepository : IRepository<AttributeTemplate, int>
    {
        Task<AttributeTemplate?> GetByIdWithItemsAsync(int id, CancellationToken ct = default);
        Task<AttributeTemplate?> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default);
        Task<IEnumerable<AttributeTemplate>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<AttributeTemplate>> GetAllActiveAsync(CancellationToken ct = default);
        Task<bool> ExistsForCategoryAsync(int categoryId, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    }
}
