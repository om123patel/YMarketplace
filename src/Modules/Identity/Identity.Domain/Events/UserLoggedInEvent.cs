using Shared.Domain.Abstractions;

namespace Identity.Domain.Events
{
    public sealed class UserLoggedInEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid UserId { get; }
        public string Email { get; }
        public DateTime LoggedInAt { get; }

        public UserLoggedInEvent(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
            LoggedInAt = DateTime.UtcNow;
        }
    }

}
