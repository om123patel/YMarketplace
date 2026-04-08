using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.ToTable("Stocks", "inventory");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Quantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.ReservedQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.LowStockThreshold)
                .IsRequired()
                .HasDefaultValue(5);

            builder.Property(x => x.TrackInventory)
                .HasDefaultValue(true);

            builder.Property(x => x.AllowBackorder)
                .HasDefaultValue(false);

            builder.Property(x => x.Status)
                .HasConversion(
                    s => s.ToString(),
                    s => Enum.Parse<StockStatus>(s))
                .HasMaxLength(20)
                .HasDefaultValue(StockStatus.OutOfStock);

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            builder.HasMany(x => x.Reservations)
                .WithOne()
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ProductId)
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(x => new { x.ProductId, x.VariantId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(x => x.Status)
                .HasFilter("[IsDeleted] = 0");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}