using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;

namespace Identity.Infrastructure.Persistence.Repositories
{
    public class UserRepository
    : BaseRepository<User, Guid, IdentityDbContext>, IUserRepository
    {
        public UserRepository(IdentityDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(
            string email, CancellationToken ct = default)
            => await DbSet
                .FirstOrDefaultAsync(u =>
                    u.Email == email.ToLowerInvariant(), ct);

        public async Task<User?> GetByIdWithTokensAsync(
            Guid id, CancellationToken ct = default)
            => await DbSet
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id, ct);

        public async Task<User?> GetByRefreshTokenAsync(
            string token, CancellationToken ct = default)
            => await DbSet
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens.Any(t => t.Token == token), ct);

        public async Task<bool> EmailExistsAsync(
            string email, CancellationToken ct = default)
            => await DbSet.AnyAsync(u =>
                u.Email == email.ToLowerInvariant(), ct);
    }

}
