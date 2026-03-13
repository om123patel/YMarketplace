namespace Shared.Domain.Abstractions
{
    public abstract class Entity<TId>
    {
        public TId Id { get; protected set; } = default!;

        // Audit
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
        public Guid CreatedBy { get; protected set; }
        public Guid? UpdatedBy { get; protected set; }

        // Soft delete
        public bool IsDeleted { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }

        protected Entity() { }

        protected Entity(TId id)
        {
            Id = id;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCreatedBy(Guid userId)
        {
            CreatedBy = userId;
        }

        public void SetUpdatedBy(Guid userId)
        {
            UpdatedBy = userId;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void SoftDelete(Guid deletedBy)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            UpdatedBy = deletedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity<TId> other) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id!.Equals(other.Id);
        }

        public override int GetHashCode() => Id!.GetHashCode();

        public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
            => a is null ? b is null : a.Equals(b);

        public static bool operator !=(Entity<TId>? a, Entity<TId>? b)
            => !(a == b);
    }
}
