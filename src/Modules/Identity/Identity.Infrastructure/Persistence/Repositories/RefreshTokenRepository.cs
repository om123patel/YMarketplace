using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IdentityDbContext _db;

        public RefreshTokenRepository(IdentityDbContext db) => _db = db;

        public async Task<RefreshToken?> GetByTokenAsync(
            string token, CancellationToken ct = default)
            => await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token, ct);

        public async Task<List<RefreshToken>> GetActiveByUserIdAsync(
            Guid userId, CancellationToken ct = default)
            => await _db.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync(ct);

        public async Task AddAsync(
            RefreshToken token, CancellationToken ct = default)
            => await _db.RefreshTokens.AddAsync(token, ct);

        public void Update(RefreshToken token)
            => _db.RefreshTokens.Update(token);

        public async Task RevokeAllForUserAsync(
            Guid userId, string reason, CancellationToken ct = default)
        {
            var tokens = await _db.RefreshTokens
                .Where(t => t.UserId == userId &&
                            !t.IsRevoked &&
                            t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);

            foreach (var t in tokens)
                t.Revoke(reason);
        }
    }

}
