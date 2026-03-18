using Shared.Domain.Abstractions;

namespace Identity.Domain.Entities
{
    public class RefreshToken : Entity<int>
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedReason { get; private set; }
        public string? ReplacedByToken { get; private set; }
        public string? CreatedByIp { get; private set; }

        private RefreshToken() { } // EF Core

        public static RefreshToken Create(
            Guid userId,
            string token,
            DateTime expiresAt,
            Guid createdBy,
            string? createdByIp = null)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = expiresAt,
                IsRevoked = false,
                CreatedByIp = createdByIp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive() => !IsRevoked && !IsExpired();

        public void Revoke(string reason, string? replacedByToken = null)
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            RevokedReason = reason;
            ReplacedByToken = replacedByToken;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}
