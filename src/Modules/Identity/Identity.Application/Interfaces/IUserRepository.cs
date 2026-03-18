using Identity.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Identity.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task<PagedList<User>> GetPagedAsync(
            int page, int pageSize,
            string? role, string? status,
            string? search,
            CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        void Update(User user);
    }

}
