using Shared.Domain.Abstractions;

namespace Payments.Domain.Entities
{
    /// <summary>
    /// Admin-configured commission rule — e.g. Electronics = 8%, Clothing = 12%.
    /// Rules are matched by CategoryId; a null CategoryId is the global default.
    /// </summary>
    public class CommissionRule : Entity<int>
    {
        public int? CategoryId { get; private set; }   // null = global default
        public string Name { get; private set; } = string.Empty;
        public decimal RatePercent { get; private set; }   // e.g. 8.00 = 8%
        public bool IsActive { get; private set; }

        private CommissionRule() { }   // EF Core

        public static CommissionRule Create(
            string name,
            decimal ratePercent,
            Guid createdBy,
            int? categoryId = null)
        {
            if (ratePercent < 0 || ratePercent > 100)
                throw new Shared.Domain.Exceptions.DomainException(
                    "INVALID_RATE", "Commission rate must be between 0 and 100.");

            return new CommissionRule
            {
                CategoryId = categoryId,
                Name = name,
                RatePercent = ratePercent,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void Update(string name, decimal ratePercent, int? categoryId, Guid updatedBy)
        {
            if (ratePercent < 0 || ratePercent > 100)
                throw new Shared.Domain.Exceptions.DomainException(
                    "INVALID_RATE", "Commission rate must be between 0 and 100.");

            Name = name;
            RatePercent = ratePercent;
            CategoryId = categoryId;
            SetUpdatedBy(updatedBy);
        }

        public void Activate(Guid updatedBy) { IsActive = true; SetUpdatedBy(updatedBy); }
        public void Deactivate(Guid updatedBy) { IsActive = false; SetUpdatedBy(updatedBy); }
    }
}