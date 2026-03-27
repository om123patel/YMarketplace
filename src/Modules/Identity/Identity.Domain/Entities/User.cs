using Identity.Domain.Enums;
using Shared.Domain.Abstractions;
using Shared.Domain.Exceptions;

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

        // Login tracking
        public DateTime? LastLoginAt { get; private set; }
        public int FailedLoginAttempts { get; private set; }
        public DateTime? LockedUntil { get; private set; }

        // Optimistic concurrency
        public byte[]? RowVersion { get; private set; }

        public ICollection<Seller> Sellers { get; set; } = new List<Seller>();

        private User() { } // EF Core

        // ── Factory ────────────────────────────────────────────
        public static User Create(
            string firstName,
            string lastName,
            string email,
            string passwordHash,
            UserRole role,
            Guid createdBy,
            string? phoneNumber = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("INVALID_EMAIL", "Email is required.");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new DomainException("INVALID_PASSWORD", "Password hash is required.");

            return new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                PhoneNumber = phoneNumber,
                Role = role,
                Status = role == UserRole.Buyer
                                    ? UserStatus.Active
                                    : UserStatus.PendingVerification,
                FailedLoginAttempts = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public string FullName => $"{FirstName} {LastName}";

        // ── Domain behaviours ──────────────────────────────────
        public void RecordSuccessfulLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            FailedLoginAttempts = 0;
            LockedUntil = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordFailedLogin(int maxAttempts = 5)
        {
            FailedLoginAttempts++;

            if (FailedLoginAttempts >= maxAttempts)
                LockedUntil = DateTime.UtcNow.AddMinutes(15);

            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsLockedOut()
            => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

        public void Activate(Guid updatedBy)
        {
            Status = UserStatus.Active;
            UpdatedAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);
        }

        public void Suspend(Guid updatedBy)
        {
            if (Status == UserStatus.Suspended)
                throw new DomainException("ALREADY_SUSPENDED", "User is already suspended.");

            Status = UserStatus.Suspended;
            UpdatedAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);
        }

        public void Deactivate(Guid updatedBy)
        {
            Status = UserStatus.Inactive;
            UpdatedAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);
        }

        public void UpdateProfile(
            string firstName, string lastName,
            string? phoneNumber, Guid updatedBy)
        {
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            PhoneNumber = phoneNumber;
            UpdatedAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);
        }

        public void UpdatePasswordHash(string newHash, Guid updatedBy)
        {
            PasswordHash = newHash;
            UpdatedAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);
        }

        public void SetAvatarUrl(string url, Guid updatedBy)
        {
            AvatarUrl = url;
            UpdatedAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);
        }
    }


}
