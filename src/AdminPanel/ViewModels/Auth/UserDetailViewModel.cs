namespace AdminPanel.ViewModels.Auth
{
    public class UserDetailViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Action permissions
        public bool CanSuspend { get; set; }
        public bool CanActivate { get; set; }
        public bool CanDelete { get; set; }

        // Helpers
        public bool IsLocked =>
            LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }
}
