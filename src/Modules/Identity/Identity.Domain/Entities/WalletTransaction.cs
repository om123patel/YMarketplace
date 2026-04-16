using Identity.Domain.Enums;
using Shared.Domain.Abstractions;

namespace Identity.Domain.Entities
{
    public class WalletTransaction : Entity<Guid>
    {
        public Guid WalletId { get; private set; }
        public decimal Amount { get; private set; }
        public WalletTransactionType Type { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public string? ReferenceId { get; private set; } // order id, refund id etc.
        public decimal BalanceAfter { get; private set; }

        private WalletTransaction() { }

        public static WalletTransaction Create(
            Guid walletId, decimal amount, WalletTransactionType type,
            string description, string? referenceId,
            decimal balanceAfter, Guid createdBy)
        {
            return new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = walletId,
                Amount = amount,
                Type = type,
                Description = description,
                ReferenceId = referenceId,
                BalanceAfter = balanceAfter,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }
    }
}
