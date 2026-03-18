using Identity.Domain.Entities;

namespace Identity.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(
            string token, CancellationToken ct = default);
        Task<List<RefreshToken>> GetActiveByUserIdAsync(
            Guid userId, CancellationToken ct = default);
        Task AddAsync(RefreshToken token, CancellationToken ct = default);
        void Update(RefreshToken token);
        Task RevokeAllForUserAsync(
            Guid userId, string reason, CancellationToken ct = default);
    }
}
