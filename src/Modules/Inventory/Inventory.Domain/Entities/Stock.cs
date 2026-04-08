using Inventory.Domain.Enums;
using Shared.Domain.Abstractions;

namespace Inventory.Domain.Entities
{
    public class Stock : Entity<Guid>, IConcurrencyToken
    {
        public Guid ProductId { get; private set; }
        public Guid? VariantId { get; private set; }
        public int Quantity { get; private set; }
        public int ReservedQuantity { get; private set; }
        public int LowStockThreshold { get; private set; }
        public bool TrackInventory { get; private set; }
        public bool AllowBackorder { get; private set; }
        public StockStatus Status { get; private set; }

        public byte[]? RowVersion { get; private set; }

        // Navigation
        public ICollection<StockReservation> Reservations { get; private set; } = [];

        private Stock() { }

        public static Stock Create(
            Guid productId,
            Guid createdBy,
            Guid? variantId = null,
            int initialQuantity = 0,
            int lowStockThreshold = 5,
            bool trackInventory = true,
            bool allowBackorder = false)
        {
            var stock = new Stock
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                VariantId = variantId,
                Quantity = initialQuantity,
                ReservedQuantity = 0,
                LowStockThreshold = lowStockThreshold,
                TrackInventory = trackInventory,
                AllowBackorder = allowBackorder,
                Status = initialQuantity > 0 ? StockStatus.InStock : StockStatus.OutOfStock,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
            return stock;
        }

        public int AvailableQuantity => Quantity - ReservedQuantity;

        public void AddStock(int quantity, Guid updatedBy)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity to add must be positive.", nameof(quantity));

            Quantity += quantity;
            RecalculateStatus();
            SetUpdatedBy(updatedBy);
        }

        public void SetQuantity(int quantity, Guid updatedBy)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));

            Quantity = quantity;
            RecalculateStatus();
            SetUpdatedBy(updatedBy);
        }

        public void Reserve(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Reservation quantity must be positive.");

            if (!AllowBackorder && quantity > AvailableQuantity)
                throw new InvalidOperationException(
                    $"Insufficient stock. Available: {AvailableQuantity}, Requested: {quantity}");

            ReservedQuantity += quantity;
            RecalculateStatus();
            UpdatedAt = DateTime.UtcNow;
        }

        public void ReleaseReservation(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Release quantity must be positive.");

            ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
            RecalculateStatus();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deduct(int quantity, Guid updatedBy)
        {
            if (quantity <= 0)
                throw new ArgumentException("Deduct quantity must be positive.");

            Quantity = Math.Max(0, Quantity - quantity);
            ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
            RecalculateStatus();
            SetUpdatedBy(updatedBy);
        }

        public void UpdateSettings(
            int lowStockThreshold,
            bool trackInventory,
            bool allowBackorder,
            Guid updatedBy)
        {
            LowStockThreshold = lowStockThreshold;
            TrackInventory = trackInventory;
            AllowBackorder = allowBackorder;
            RecalculateStatus();
            SetUpdatedBy(updatedBy);
        }

        public bool IsLowStock =>
            TrackInventory &&
            AvailableQuantity > 0 &&
            AvailableQuantity <= LowStockThreshold;

        private void RecalculateStatus()
        {
            if (!TrackInventory)
            {
                Status = StockStatus.InStock;
                return;
            }

            if (AvailableQuantity <= 0 && !AllowBackorder)
                Status = StockStatus.OutOfStock;
            else if (AvailableQuantity <= LowStockThreshold && AvailableQuantity > 0)
                Status = StockStatus.LowStock;
            else
                Status = StockStatus.InStock;
        }
    }
}