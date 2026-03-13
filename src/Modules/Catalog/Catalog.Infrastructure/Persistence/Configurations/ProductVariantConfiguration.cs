using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Sku)
                .HasMaxLength(100);

            builder.Property(x => x.Barcode)
                .HasMaxLength(100);

            // Money value objects
            builder.OwnsOne(x => x.Price, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Price")
                    .HasColumnType("decimal(18,4)")
                    .IsRequired();

                money.Property(m => m.CurrencyCode)
                    .HasColumnName("CurrencyCode")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("USD");
            });

            builder.OwnsOne(x => x.CompareAtPrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("CompareAtPrice")
                    .HasColumnType("decimal(18,4)");

                money.Property(m => m.CurrencyCode)
                    .HasColumnName("CompareAtPriceCurrency")
                    .HasMaxLength(3);
            });

            builder.OwnsOne(x => x.CostPrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("CostPrice")
                    .HasColumnType("decimal(18,4)");

                money.Property(m => m.CurrencyCode)
                    .HasColumnName("CostPriceCurrency")
                    .HasMaxLength(3);
            });

            builder.Property(x => x.WeightKg)
                .HasColumnType("decimal(10,3)");

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            // Relationship to attributes
            builder.HasMany(x => x.Attributes)
                .WithOne()
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ProductId)
                .HasFilter("[IsDeleted] = 0");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }

}
