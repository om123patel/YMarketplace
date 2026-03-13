using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public class ProductSeoConfiguration : IEntityTypeConfiguration<ProductSeo>
    {
        public void Configure(EntityTypeBuilder<ProductSeo> builder)
        {
            builder.ToTable("ProductSeo", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseIdentityColumn();

            builder.Property(x => x.MetaTitle)
                .HasMaxLength(70);

            builder.Property(x => x.MetaDescription)
                .HasMaxLength(165);

            builder.Property(x => x.MetaKeywords)
                .HasMaxLength(500);

            builder.Property(x => x.CanonicalUrl)
                .HasMaxLength(500);

            builder.Property(x => x.OgTitle)
                .HasMaxLength(200);

            builder.Property(x => x.OgDescription)
                .HasMaxLength(300);

            builder.Property(x => x.OgImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => x.ProductId)
                .IsUnique();
        }
    }

}
