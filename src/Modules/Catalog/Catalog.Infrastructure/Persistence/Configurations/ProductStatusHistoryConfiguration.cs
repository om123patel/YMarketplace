using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public class ProductStatusHistoryConfiguration
     : IEntityTypeConfiguration<ProductStatusHistory>
    {
        public void Configure(EntityTypeBuilder<ProductStatusHistory> builder)
        {
            builder.ToTable("ProductStatusHistory", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseIdentityColumn();

            builder.Property(x => x.FromStatus)
                .HasConversion(
                    s => s.HasValue ? s.Value.ToString() : null,
                    s => s != null ? Enum.Parse<ProductStatus>(s) : (ProductStatus?)null)
                .HasMaxLength(30);

            builder.Property(x => x.ToStatus)
                .HasConversion(
                    s => s.ToString(),
                    s => Enum.Parse<ProductStatus>(s))
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.Note)
                .HasMaxLength(1000);

            builder.Property(x => x.ChangedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => x.ProductId);
        }
    }

}
