using Shared.Domain.Abstractions;

namespace Identity.Domain.Events
{
    public sealed class UserRegisteredEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid UserId { get; }
        public string Email { get; }
        public string Role { get; }

        public UserRegisteredEvent(Guid userId, string email, string role)
        {
            UserId = userId;
            Email = email;
            Role = role;
        }
    }
}
