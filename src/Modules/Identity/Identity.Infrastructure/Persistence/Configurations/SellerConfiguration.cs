using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations
{
    public class SellerConfiguration : IEntityTypeConfiguration<Seller>
    {
        public void Configure(EntityTypeBuilder<Seller> builder)
        {
            builder.ToTable("Sellers", "identity");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BusinessName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.BusinessEmail)
                .HasMaxLength(255);

            builder.Property(x => x.BusinessPhone)
                .HasMaxLength(20);

            // ✅ FIX 1: Correct column name
            builder.Property(x => x.Status)
                .HasColumnName("SellerStatus")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            // ✅ FIX 3: Address mapping
            builder.OwnsOne(x => x.Address, address =>
            {
                address.Property(a => a.AddressLine1).HasColumnName("AddressLine1");
                address.Property(a => a.AddressLine2).HasColumnName("AddressLine2");
                address.Property(a => a.City).HasColumnName("City");
                address.Property(a => a.State).HasColumnName("State");
                address.Property(a => a.PostalCode).HasColumnName("PostalCode");
                address.Property(a => a.Country).HasColumnName("Country");
            });

            // ✅ FIX 2: FK mapping
            builder.HasOne(x => x.User)
            .WithMany() 
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("FK_Sellers_Users")
            .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.TotalRevenue)
                .HasColumnType("decimal(18,4)");

            builder.Property(x => x.Rating)
                .HasColumnType("decimal(3,2)");

            builder.Property(x => x.RowVersion)
                .IsRowVersion();
        }
    }
}
