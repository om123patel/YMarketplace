using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations
{
    public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
    {
        public void Configure(EntityTypeBuilder<StockReservation> builder)
        {
            builder.ToTable("StockReservations", "inventory");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion(
                    s => s.ToString(),
                    s => Enum.Parse<ReservationStatus>(s))
                .HasMaxLength(20)
                .HasDefaultValue(ReservationStatus.Active);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => x.OrderId);
            builder.HasIndex(x => x.StockId);
            builder.HasIndex(x => new { x.OrderId, x.Status });
        }
    }
}