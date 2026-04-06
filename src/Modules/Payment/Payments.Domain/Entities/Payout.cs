using Payments.Domain.Enums;
using Payments.Domain.Events;
using Payments.Domain.Exceptions;
using Shared.Domain.Abstractions;

namespace Payments.Domain.Entities
{
    /// <summary>
    /// Represents a seller earnings payout request and its lifecycle.
    /// </summary>
    public class Payout : AggregateRoot<Guid>, IConcurrencyToken
    {
        public Guid SellerId { get; private set; }
        public decimal Amount { get; private set; }
        public string CurrencyCode { get; private set; } = "INR";
        public PayoutStatus Status { get; private set; }

        // Bank / UPI details captured at request time
        public string? BankAccountNumber { get; private set; }
        public string? BankIfscCode { get; private set; }
        public string? BankAccountName { get; private set; }
        public string? UpiId { get; private set; }

        // Admin processing
        public Guid? ProcessedByAdminId { get; private set; }
        public string? AdminNote { get; private set; }
        public string? FailureReason { get; private set; }
        public string? GatewayReference { get; private set; }

        // Timestamps
        public DateTime? ProcessedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime? FailedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }

        public byte[]? RowVersion { get; private set; }

        // Navigation — which transactions are covered by this payout
        public ICollection<PayoutTransaction> PayoutTransactions { get; private set; } = [];

        private Payout() { }   // EF Core

        // ── Factory ──────────────────────────────────────────────
        public static Payout Create(
            Guid sellerId,
            decimal amount,
            string currencyCode,
            Guid createdBy,
            string? bankAccountNumber = null,
            string? bankIfscCode = null,
            string? bankAccountName = null,
            string? upiId = null)
        {
            if (amount <= 0)
                throw new PaymentsException(
                    "INVALID_PAYOUT_AMOUNT",
                    "Payout amount must be greater than zero.");

            return new Payout
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                Amount = amount,
                CurrencyCode = currencyCode.ToUpperInvariant(),
                Status = PayoutStatus.Pending,
                BankAccountNumber = bankAccountNumber,
                BankIfscCode = bankIfscCode,
                BankAccountName = bankAccountName,
                UpiId = upiId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        // ── State machine ─────────────────────────────────────────
        public void StartProcessing(Guid adminId, string? note = null)
        {
            if (Status != PayoutStatus.Pending)
                throw new InvalidPayoutStatusException(Status, PayoutStatus.Processing);

            Status = PayoutStatus.Processing;
            ProcessedByAdminId = adminId;
            AdminNote = note;
            ProcessedAt = DateTime.UtcNow;
            SetUpdatedBy(adminId);
        }

        public void MarkCompleted(string gatewayReference, Guid adminId)
        {
            if (Status != PayoutStatus.Processing)
                throw new InvalidPayoutStatusException(Status, PayoutStatus.Completed);

            Status = PayoutStatus.Completed;
            GatewayReference = gatewayReference;
            CompletedAt = DateTime.UtcNow;
            SetUpdatedBy(adminId);

            RaiseDomainEvent(new PayoutProcessedEvent(Id, SellerId, Amount, CurrencyCode));
        }

        public void MarkFailed(string reason, Guid adminId)
        {
            if (Status != PayoutStatus.Processing)
                throw new InvalidPayoutStatusException(Status, PayoutStatus.Failed);

            Status = PayoutStatus.Failed;
            FailureReason = reason;
            FailedAt = DateTime.UtcNow;
            SetUpdatedBy(adminId);
        }

        public void Cancel(string reason, Guid cancelledBy)
        {
            if (Status is PayoutStatus.Completed or PayoutStatus.Failed)
                throw new PaymentsException(
                    "INVALID_PAYOUT_STATUS_TRANSITION",
                    "Completed or failed payouts cannot be cancelled.");

            Status = PayoutStatus.Cancelled;
            FailureReason = reason;
            CancelledAt = DateTime.UtcNow;
            SetUpdatedBy(cancelledBy);
        }
    }
}