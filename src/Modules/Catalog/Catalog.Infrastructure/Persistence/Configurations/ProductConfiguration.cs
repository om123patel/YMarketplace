using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            // Core
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(350);

            builder.Property(x => x.ShortDescription)
                .HasMaxLength(500);

            builder.Property(x => x.Description)
                .HasColumnType("nvarchar(max)");

            // Pricing — Money value object mapped as owned entity
            builder.OwnsOne(x => x.BasePrice, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("BasePrice")
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

            // Identifiers
            builder.Property(x => x.Sku)
                .HasMaxLength(100);

            builder.Property(x => x.Barcode)
                .HasMaxLength(100);

            // Physical
            builder.Property(x => x.WeightKg)
                .HasColumnType("decimal(10,3)");

            builder.Property(x => x.LengthCm)
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.WidthCm)
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.HeightCm)
                .HasColumnType("decimal(10,2)");

            // Flags
            builder.Property(x => x.IsDigital)
                .HasDefaultValue(false);

            builder.Property(x => x.RequiresShipping)
                .HasDefaultValue(true);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(false);

            builder.Property(x => x.IsFeatured)
                .HasDefaultValue(false);

            // Status — store as string
            builder.Property(x => x.Status)
                .HasConversion(
                    s => s.ToString(),
                    s => Enum.Parse<ProductStatus>(s))
                .HasMaxLength(30)
                .HasDefaultValue(ProductStatus.Draft);

            // Creator type
            builder.Property(x => x.CreatorType)
                .HasConversion(
                    c => c.ToString(),
                    c => Enum.Parse<ProductCreatorType>(c))
                .HasMaxLength(20);

            // Approval
            builder.Property(x => x.RejectionReason)
                .HasMaxLength(1000);

            // Audit
            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            // Relationships
            builder.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Brand)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Seo)
                .WithOne()
                .HasForeignKey<ProductSeo>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Images)
                .WithOne()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Variants)
                .WithOne()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many Tags — explicitly name FK columns to avoid EF Core
            // auto-convention generating "ProductsId" / "TagsId"
            builder.HasMany(x => x.Tags)
                .WithMany(x => x.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductTag",
                    r => r.HasOne<Tag>()
                          .WithMany()
                          .HasForeignKey("TagId")
                          .OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Product>()
                          .WithMany()
                          .HasForeignKey("ProductId")
                          .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.ToTable("ProductTags", "catalog");
                        j.HasKey("ProductId", "TagId");
                    });

            // Indexes
            builder.HasIndex(x => x.Slug)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(x => x.Status)
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(x => x.SellerId)
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(x => x.StoreId)
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(x => x.CategoryId)
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(x => x.IsFeatured)
                .HasFilter("[IsDeleted] = 0 AND [IsActive] = 1");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }

}