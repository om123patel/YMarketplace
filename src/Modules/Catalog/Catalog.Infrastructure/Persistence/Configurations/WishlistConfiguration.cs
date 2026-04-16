// src/Modules/Catalog/Catalog.Infrastructure/Persistence/Configurations/WishlistConfiguration.cs

using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.ToTable("Wishlists", "catalog");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.BuyerId)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }

  
}