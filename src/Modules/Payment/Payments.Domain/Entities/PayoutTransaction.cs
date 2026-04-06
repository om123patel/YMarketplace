using Shared.Domain.Abstractions;

namespace Payments.Domain.Entities
{
    /// <summary>Junction: which Transactions are covered by a Payout.</summary>
    public class PayoutTransaction : Entity<int>
    {
        public Guid PayoutId { get; private set; }
        public Guid TransactionId { get; private set; }

        private PayoutTransaction() { }

        public static PayoutTransaction Create(
            Guid payoutId,
            Guid transactionId,
            Guid createdBy)
            => new()
            {
                PayoutId = payoutId,
                TransactionId = transactionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
    }
}