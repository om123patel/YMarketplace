using Shared.Domain.Abstractions;

namespace Identity.Domain.Events
{
    public sealed class VendorApprovedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public Guid UserId { get; }
        public Guid ApplicationId { get; }
        public string StoreName { get; }

        public VendorApprovedEvent(Guid userId, Guid applicationId, string storeName)
        {
            UserId = userId;
            ApplicationId = applicationId;
            StoreName = storeName;
        }
    }
}
