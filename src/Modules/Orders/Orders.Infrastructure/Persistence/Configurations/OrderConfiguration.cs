using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders", "orders");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(30);
            builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3);

            builder.Property(x => x.SubTotal).HasColumnType("decimal(18,4)");
            builder.Property(x => x.ShippingAmount).HasColumnType("decimal(18,4)");
            builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,4)");
            builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,4)");

            builder.Property(x => x.Status)
                .HasConversion(s => s.ToString(), s => Enum.Parse<OrderStatus>(s))
                .HasMaxLength(20).HasDefaultValue(OrderStatus.Pending);

            builder.Property(x => x.PaymentStatus)
                .HasConversion(s => s.ToString(), s => Enum.Parse<PaymentStatus>(s))
                .HasMaxLength(25).HasDefaultValue(PaymentStatus.Pending);

            // Shipping address columns
            builder.Property(x => x.ShippingName).HasMaxLength(150);
            builder.Property(x => x.ShippingAddressLine1).HasMaxLength(200);
            builder.Property(x => x.ShippingAddressLine2).HasMaxLength(200);
            builder.Property(x => x.ShippingCity).HasMaxLength(100);
            builder.Property(x => x.ShippingState).HasMaxLength(100);
            builder.Property(x => x.ShippingPostalCode).HasMaxLength(20);
            builder.Property(x => x.ShippingCountry).HasMaxLength(100);
            builder.Property(x => x.ShippingPhone).HasMaxLength(20);

            // Fulfillment
            builder.Property(x => x.TrackingNumber).HasMaxLength(100);
            builder.Property(x => x.ShippingCarrier).HasMaxLength(100);
            builder.Property(x => x.TrackingUrl).HasMaxLength(500);
            builder.Property(x => x.CancellationReason).HasMaxLength(1000);

            builder.Property(x => x.RowVersion).IsRowVersion();
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Disputes)
                .WithOne()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.OrderNumber).IsUnique();
            builder.HasIndex(x => x.BuyerId).HasFilter("[IsDeleted] = 0");
            builder.HasIndex(x => x.StoreId).HasFilter("[IsDeleted] = 0");
            builder.HasIndex(x => x.Status).HasFilter("[IsDeleted] = 0");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }

}
