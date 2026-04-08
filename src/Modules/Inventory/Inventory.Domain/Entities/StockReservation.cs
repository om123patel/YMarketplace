using Inventory.Domain.Enums;
using Shared.Domain.Abstractions;

namespace Inventory.Domain.Entities
{
    public class StockReservation : Entity<Guid>
    {
        public Guid StockId { get; private set; }
        public Guid OrderId { get; private set; }
        public int Quantity { get; private set; }
        public ReservationStatus Status { get; private set; }
        public DateTime? ReleasedAt { get; private set; }
        public DateTime? ConfirmedAt { get; private set; }

        private StockReservation() { }

        public static StockReservation Create(
            Guid stockId,
            Guid orderId,
            int quantity,
            Guid createdBy)
        {
            return new StockReservation
            {
                Id = Guid.NewGuid(),
                StockId = stockId,
                OrderId = orderId,
                Quantity = quantity,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }

        public void Release(Guid updatedBy)
        {
            Status = ReservationStatus.Released;
            ReleasedAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);
        }

        public void Confirm(Guid updatedBy)
        {
            Status = ReservationStatus.Confirmed;
            ConfirmedAt = DateTime.UtcNow;
            SetUpdatedBy(updatedBy);
        }
    }
}