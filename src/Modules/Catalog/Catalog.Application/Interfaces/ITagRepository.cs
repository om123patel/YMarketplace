using Catalog.Application.DTOs.Tags;
using Catalog.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Catalog.Application.Interfaces
{
    public interface ITagRepository : IRepository<Tag, int>
    {
        Task<PagedList<Tag>> GetPagedAsync(TagFilterRequest filter, CancellationToken ct = default);
        Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
        Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
        Task<bool> NameExistsAsync(string name, CancellationToken ct = default);
        Task<bool> SlugExistsExceptAsync(string slug, int excludeId, CancellationToken ct = default);
        Task<bool> NameExistsExceptAsync(string name, int excludeId, CancellationToken ct = default);
    }
}
