using Identity.Domain.Enums;
using Identity.Domain.Events;
using Identity.Domain.Exceptions;
using Shared.Domain.Abstractions;

namespace Identity.Domain.Entities
{
    public class User : AggregateRoot<Guid>, IConcurrencyToken
    {
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string? PhoneNumber { get; private set; }
        public string? AvatarUrl { get; private set; }
        public UserRole Role { get; private set; }
        public UserStatus Status { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public int FailedLoginAttempts { get; private set; }
        public DateTime? LockedUntil { get; private set; }

        // IConcurrencyToken
        public byte[]? RowVersion { get; private set; }

        // Computed
        public string FullName => $"{FirstName} {LastName}";
        public bool IsLocked => LockedUntil.HasValue
            && LockedUntil.Value > DateTime.UtcNow;

        // Navigation
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

        private User() { } // EF Core

        // ── Factory ──
        public static User Create(
            string firstName,
            string lastName,
            string email,
            string passwordHash,
            UserRole role,
            string? phoneNumber = null)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email.ToLowerInvariant(),
                PasswordHash = passwordHash,
                PhoneNumber = phoneNumber,
                Role = role,
                Status = UserStatus.Active,
                FailedLoginAttempts = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty // set after save
            };

            user.RaiseDomainEvent(
                new UserRegisteredEvent(user.Id, user.Email, role.ToString()));

            return user;
        }

        // ── Login ──
        public void RecordLogin()
        {
            if (IsLocked)
                throw new IdentityException(
                    "ACCOUNT_LOCKED",
                    $"Account is locked until {LockedUntil:yyyy-MM-dd HH:mm} UTC.");

            if (Status != UserStatus.Active)
                throw new IdentityException(
                    "ACCOUNT_INACTIVE",
                    "Account is not active.");

            LastLoginAt = DateTime.UtcNow;
            FailedLoginAttempts = 0;
            UpdatedAt = DateTime.UtcNow;

            RaiseDomainEvent(new UserLoggedInEvent(Id, Email));
        }

        public void RecordFailedLogin()
        {
            FailedLoginAttempts++;

            // Lock account after 5 failed attempts for 15 minutes
            if (FailedLoginAttempts >= 5)
            {
                LockedUntil = DateTime.UtcNow.AddMinutes(15);
            }

            UpdatedAt = DateTime.UtcNow;
        }

        // ── Refresh Tokens ──
        public RefreshToken AddRefreshToken(
            string token,
            DateTime expiresAt,
            string? ipAddress = null)
        {
            // Revoke all existing active refresh tokens — one active token per user
            foreach (var existingToken in RefreshTokens.Where(t => t.IsActive))
                existingToken.Revoke("Replaced by new token.");

            var refreshToken = RefreshToken.Create(Id, token, expiresAt, ipAddress);
            RefreshTokens.Add(refreshToken);

            return refreshToken;
        }

        public RefreshToken? GetActiveRefreshToken(string token)
            => RefreshTokens.FirstOrDefault(t => t.Token == token && t.IsActive);

        public void RevokeRefreshToken(string token, string reason)
        {
            var refreshToken = RefreshTokens.FirstOrDefault(t => t.Token == token);
            refreshToken?.Revoke(reason);
        }

        // ── Profile ──
        public void UpdateProfile(
            string firstName,
            string lastName,
            string? phoneNumber,
            string? avatarUrl,
            Guid updatedBy)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            AvatarUrl = avatarUrl;
            SetUpdatedBy(updatedBy);
        }

        public void ChangePassword(string newPasswordHash, Guid updatedBy)
        {
            PasswordHash = newPasswordHash;

            // Revoke all refresh tokens on password change
            foreach (var token in RefreshTokens.Where(t => t.IsActive))
                token.Revoke("Password changed.");

            SetUpdatedBy(updatedBy);
        }

        // ── Status ──
        public void Activate(Guid updatedBy)
        {
            Status = UserStatus.Active;
            SetUpdatedBy(updatedBy);
        }

        public void Suspend(Guid updatedBy)
        {
            Status = UserStatus.Suspended;

            // Revoke all refresh tokens on suspension
            foreach (var token in RefreshTokens.Where(t => t.IsActive))
                token.Revoke("Account suspended.");

            SetUpdatedBy(updatedBy);
        }

        public void Deactivate(Guid updatedBy)
        {
            Status = UserStatus.Inactive;
            SetUpdatedBy(updatedBy);
        }

        public void UnlockAccount(Guid updatedBy)
        {
            LockedUntil = null;
            FailedLoginAttempts = 0;
            SetUpdatedBy(updatedBy);
        }
    }

}
