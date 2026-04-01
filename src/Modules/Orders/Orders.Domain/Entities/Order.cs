using Orders.Domain.Enums;
using Orders.Domain.Events;
using Orders.Domain.Exceptions;
using Shared.Domain.Abstractions;

namespace Orders.Domain.Entities
{
    public class Order : AggregateRoot<Guid>, IConcurrencyToken
    {
        public string OrderNumber { get; private set; } = string.Empty;
        public Guid BuyerId { get; private set; }
        public Guid StoreId { get; private set; }
        public Guid SellerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public PaymentStatus PaymentStatus { get; private set; }

        // Pricing
        public decimal SubTotal { get; private set; }
        public decimal ShippingAmount { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string CurrencyCode { get; private set; } = string.Empty;

        // Shipping address snapshot
        public string? ShippingName { get; private set; }
        public string? ShippingAddressLine1 { get; private set; }
        public string? ShippingAddressLine2 { get; private set; }
        public string? ShippingCity { get; private set; }
        public string? ShippingState { get; private set; }
        public string? ShippingPostalCode { get; private set; }
        public string? ShippingCountry { get; private set; }
        public string? ShippingPhone { get; private set; }

        // Fulfillment
        public string? TrackingNumber { get; private set; }
        public string? TrackingUrl { get; private set; }
        public string? ShippingCarrier { get; private set; }
        public DateTime? ShippedAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }

        // Concurrency
        public byte[]? RowVersion { get; private set; }

        // Navigation
        public ICollection<OrderItem> Items { get; private set; } = [];
        public ICollection<Dispute> Disputes { get; private set; } = [];

        private Order() { } // EF Core

        // ── Factory ──────────────────────────────────────────
        public static Order Place(
            Guid buyerId,
            Guid storeId,
            Guid sellerId,
            string currencyCode,
            decimal shippingAmount,
            string? shippingName,
            string? addressLine1,
            string? addressLine2,
            string? city,
            string? state,
            string? postalCode,
            string? country,
            string? phone,
            IEnumerable<(Guid ProductId, Guid? VariantId, string ProductName,
                         string? VariantName, string? ImageUrl,
                         int Quantity, decimal UnitPrice)> items,
            Guid createdBy)
        {
            var itemList = items.ToList();
            if (!itemList.Any())
                throw new OrdersException("ORDER_EMPTY", "An order must have at least one item.");

            var subTotal = itemList.Sum(i => i.UnitPrice * i.Quantity);
            var totalAmount = subTotal + shippingAmount;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                BuyerId = buyerId,
                StoreId = storeId,
                SellerId = sellerId,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                SubTotal = subTotal,
                ShippingAmount = shippingAmount,
                DiscountAmount = 0,
                TotalAmount = totalAmount,
                CurrencyCode = currencyCode,
                ShippingName = shippingName,
                ShippingAddressLine1 = addressLine1,
                ShippingAddressLine2 = addressLine2,
                ShippingCity = city,
                ShippingState = state,
                ShippingPostalCode = postalCode,
                ShippingCountry = country,
                ShippingPhone = phone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            foreach (var item in itemList)
            {
                order.Items.Add(OrderItem.Create(
                    order.Id, item.ProductId, item.VariantId,
                    item.ProductName, item.VariantName, item.ImageUrl,
                    item.Quantity, item.UnitPrice, currencyCode, createdBy));
            }

            order.RaiseDomainEvent(new OrderPlacedEvent(
                order.Id,
                buyerId,
                storeId,
                itemList.Select(i => new OrderPlacedItem(
                    i.ProductId, i.VariantId, i.Quantity, i.UnitPrice))
                    .ToList()
                    .AsReadOnly(),
                totalAmount,
                currencyCode));

            return order;
        }

        // ── Status transitions ────────────────────────────────

        public void Confirm(Guid updatedBy)
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOrderStatusTransitionException(Status, OrderStatus.Confirmed);

            var previous = Status;
            Status = OrderStatus.Confirmed;
            SetUpdatedBy(updatedBy);
            RaiseDomainEvent(new OrderStatusChangedEvent(Id, previous, Status, updatedBy));
        }

        public void MarkAsShipped(
            string trackingNumber, string carrier, string? trackingUrl, Guid updatedBy)
        {
            if (Status != OrderStatus.Confirmed)
                throw new InvalidOrderStatusTransitionException(Status, OrderStatus.Shipped);

            var previous = Status;
            Status = OrderStatus.Shipped;
            TrackingNumber = trackingNumber;
            ShippingCarrier = carrier;
            TrackingUrl = trackingUrl;
            ShippedAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);
            RaiseDomainEvent(new OrderStatusChangedEvent(Id, previous, Status, updatedBy));
        }

        public void MarkAsDelivered(Guid updatedBy)
        {
            if (Status != OrderStatus.Shipped)
                throw new InvalidOrderStatusTransitionException(Status, OrderStatus.Delivered);

            var previous = Status;
            Status = OrderStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);

            RaiseDomainEvent(new OrderStatusChangedEvent(Id, previous, Status, updatedBy));
            RaiseDomainEvent(new OrderDeliveredEvent(Id, BuyerId, StoreId));
        }

        public void Cancel(string reason, Guid updatedBy)
        {
            if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
                throw new InvalidOrderStatusTransitionException(Status, OrderStatus.Cancelled);

            var previous = Status;
            Status = OrderStatus.Cancelled;
            CancellationReason = reason;
            CancelledAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);

            RaiseDomainEvent(new OrderStatusChangedEvent(Id, previous, Status, updatedBy));
            RaiseDomainEvent(new OrderCancelledEvent(
                Id, BuyerId, TotalAmount, CurrencyCode, reason));
        }

        public void MarkPaymentPaid(Guid updatedBy)
        {
            PaymentStatus = PaymentStatus.Paid;
            SetUpdatedBy(updatedBy);
        }

        public void MarkPaymentRefunded(Guid updatedBy)
        {
            PaymentStatus = PaymentStatus.Refunded;
            SetUpdatedBy(updatedBy);
        }

        public void ApplyDiscount(decimal discountAmount, Guid updatedBy)
        {
            if (discountAmount < 0)
                throw new OrdersException("INVALID_DISCOUNT", "Discount cannot be negative.");

            DiscountAmount = discountAmount;
            TotalAmount = SubTotal + ShippingAmount - DiscountAmount;
            SetUpdatedBy(updatedBy);
        }

        public Dispute OpenDispute(Guid buyerId, string reason, string? evidence, Guid createdBy)
        {
            if (BuyerId != buyerId)
                throw new OrdersException("UNAUTHORIZED",
                    "Only the buyer of this order can open a dispute.");

            if (Status == OrderStatus.Pending || Status == OrderStatus.Cancelled)
                throw new OrdersException("INVALID_DISPUTE",
                    "Cannot open a dispute on a pending or cancelled order.");

            if (Disputes.Any(d => d.Status != DisputeStatus.Resolved))
                throw new OrdersException("ACTIVE_DISPUTE_EXISTS",
                    "This order already has an active dispute.");

            var dispute = Dispute.Open(Id, buyerId, reason, evidence, createdBy);
            Disputes.Add(dispute);
            SetUpdatedBy(createdBy);
            return dispute;
        }

        private static string GenerateOrderNumber()
        {
            // Format: ORD-YYYYMMDD-XXXXXX
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = new Random().Next(100000, 999999);
            return $"ORD-{datePart}-{randomPart}";
        }
    }
}