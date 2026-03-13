using Identity.Domain.Entities;
using Shared.Application.Interfaces;

namespace Identity.Application.Interfaces
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetByIdWithTokensAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    }

}
