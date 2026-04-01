using Orders.Domain.Enums;
using Orders.Domain.Exceptions;
using Shared.Domain.Abstractions;

namespace Orders.Domain.Entities
{
    public class Dispute : Entity<Guid>
    {
        public Guid OrderId { get; private set; }
        public Guid BuyerId { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public string? BuyerEvidence { get; private set; }
        public string? SellerResponse { get; private set; }
        public string? AdminNote { get; private set; }
        public DisputeStatus Status { get; private set; }
        public Guid? ResolvedByAdminId { get; private set; }
        public DateTime? ResolvedAt { get; private set; }
        public string? Resolution { get; private set; }

        private Dispute() { } // EF Core

        public static Dispute Open(
            Guid orderId,
            Guid buyerId,
            string reason,
            string? evidence,
            Guid createdBy)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new OrdersException("INVALID_DISPUTE", "Dispute reason is required.");

            return new Dispute
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                BuyerId = buyerId,
                Reason = reason,
                BuyerEvidence = evidence,
                Status = DisputeStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void SubmitSellerResponse(string response, Guid sellerId)
        {
            if (Status != DisputeStatus.Open)
                throw new OrdersException("INVALID_DISPUTE_STATUS",
                    "Seller can only respond to open disputes.");

            SellerResponse = response;
            Status = DisputeStatus.UnderReview;
            SetUpdatedBy(sellerId);
        }

        public void Resolve(Guid adminId, string resolution)
        {
            if (Status == DisputeStatus.Resolved)
                throw new OrdersException("DISPUTE_ALREADY_RESOLVED",
                    "This dispute is already resolved.");

            Status = DisputeStatus.Resolved;
            ResolvedByAdminId = adminId;
            ResolvedAt = DateTime.UtcNow;
            Resolution = resolution;
            AdminNote = resolution;
            SetUpdatedBy(adminId);
        }

        public void Escalate(Guid adminId, string note)
        {
            if (Status != DisputeStatus.UnderReview)
                throw new OrdersException("INVALID_DISPUTE_STATUS",
                    "Only disputes under review can be escalated.");

            Status = DisputeStatus.Escalated;
            AdminNote = note;
            SetUpdatedBy(adminId);
        }
    }
}