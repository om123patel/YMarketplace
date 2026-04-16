using Payments.Domain.Enums;
using Payments.Domain.Events;
using Payments.Domain.Exceptions;
using Shared.Domain.Abstractions;

namespace Payments.Domain.Entities
{
    public class Transaction : AggregateRoot<Guid>, IConcurrencyToken
    {
        public Guid OrderId { get; private set; }
        public Guid BuyerId { get; private set; }
        public Guid SellerId { get; private set; }
        public Guid StoreId { get; private set; }

        public decimal Amount { get; private set; }
        public decimal CommissionAmount { get; private set; }
        public decimal SellerAmount { get; private set; }
        public string CurrencyCode { get; private set; } = "INR";

        public TransactionStatus Status { get; private set; }
        public PaymentMethod Method { get; private set; }

        // Gateway reference
        public string? GatewayTransactionId { get; private set; }
        public string? GatewayProvider { get; private set; }  // Stripe | Razorpay
        public string? GatewayResponse { get; private set; }  // raw JSON from gateway

        // Refund
        public decimal RefundedAmount { get; private set; }
        public string? RefundReason { get; private set; }
        public DateTime? RefundedAt { get; private set; }

        // Timestamps
        public DateTime? CompletedAt { get; private set; }
        public DateTime? FailedAt { get; private set; }

        public byte[]? RowVersion { get; private set; }

        private Transaction() { }   // EF Core


        // Add to Transaction.cs — inside the class body after existing properties

        public string? GatewayOrderId { get; private set; }  // gateway-side order/payment ID
        public string? GatewayCheckoutUrl { get; private set; }  // redirect URL for hosted pages

        // Call after Create() to record the gateway-assigned order ID and checkout URL
        public void SetGatewayOrderId(string gatewayOrderId, string? checkoutUrl, Guid updatedBy)
        {
            GatewayOrderId = gatewayOrderId;
            GatewayCheckoutUrl = checkoutUrl;
            SetUpdatedBy(updatedBy);
        }

        // ── Factory ──────────────────────────────────────────────
        public static Transaction Create(
            Guid orderId,
            Guid buyerId,
            Guid sellerId,
            Guid storeId,
            decimal amount,
            decimal commissionAmount,
            string currencyCode,
            PaymentMethod method,
            Guid createdBy,
            string? gatewayProvider = null)
        {
            if (amount <= 0)
                throw new PaymentsException(
                    "INVALID_AMOUNT", "Transaction amount must be greater than zero.");

            if (commissionAmount < 0 || commissionAmount > amount)
                throw new PaymentsException(
                    "INVALID_COMMISSION",
                    "Commission amount must be between 0 and the transaction amount.");

            return new Transaction
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                BuyerId = buyerId,
                SellerId = sellerId,
                StoreId = storeId,
                Amount = amount,
                CommissionAmount = commissionAmount,
                SellerAmount = amount - commissionAmount,
                CurrencyCode = currencyCode.ToUpperInvariant(),
                Status = TransactionStatus.Pending,
                Method = method,
                GatewayProvider = gatewayProvider,
                RefundedAmount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        // ── Behaviours ────────────────────────────────────────────
        public void MarkCompleted(
            string gatewayTransactionId,
            string? gatewayResponse,
            Guid updatedBy)
        {
            if (Status != TransactionStatus.Pending)
                throw new PaymentsException(
                    "INVALID_STATUS_TRANSITION",
                    "Only pending transactions can be marked as completed.");

            Status = TransactionStatus.Completed;
            GatewayTransactionId = gatewayTransactionId;
            GatewayResponse = gatewayResponse;
            CompletedAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);

            RaiseDomainEvent(new PaymentCompletedEvent(
                Id, OrderId, BuyerId, SellerId, Amount, CurrencyCode));
        }

        public void MarkFailed(string? reason, Guid updatedBy)
        {
            if (Status != TransactionStatus.Pending)
                throw new PaymentsException(
                    "INVALID_STATUS_TRANSITION",
                    "Only pending transactions can be marked as failed.");

            Status = TransactionStatus.Failed;
            FailedAt = DateTime.UtcNow;
            GatewayResponse = reason;
            SetUpdatedBy(updatedBy);
        }

        public void Refund(decimal refundAmount, string reason, Guid refundedBy)
        {
            if (Status != TransactionStatus.Completed)
                throw new PaymentsException(
                    "INVALID_STATUS_TRANSITION",
                    "Only completed transactions can be refunded.");

            if (refundAmount <= 0 || refundAmount > (Amount - RefundedAmount))
                throw new PaymentsException(
                    "INVALID_REFUND_AMOUNT",
                    $"Refund amount must be between 0 and {Amount - RefundedAmount}.");

            RefundedAmount += refundAmount;
            RefundReason = reason;
            RefundedAt = DateTime.UtcNow;
            Status = RefundedAmount >= Amount
                ? TransactionStatus.Refunded
                : TransactionStatus.PartiallyRefunded;

            SetUpdatedBy(refundedBy);

            RaiseDomainEvent(new PaymentRefundedEvent(
                Id, OrderId, refundAmount, CurrencyCode));
        }
    }
}