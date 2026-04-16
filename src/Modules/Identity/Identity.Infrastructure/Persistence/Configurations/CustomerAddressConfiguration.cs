// src/Modules/Identity/Identity.Infrastructure/Persistence/Configurations/CustomerAddressConfiguration.cs

using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations
{
    public class CustomerAddressConfiguration
        : IEntityTypeConfiguration<CustomerAddress>
    {
        public void Configure(EntityTypeBuilder<CustomerAddress> builder)
        {
            builder.ToTable("CustomerAddresses", "identity");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.AddressLine1)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.AddressLine2)
                .HasMaxLength(200);

            builder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.State)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.PostalCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            builder.Property(x => x.Label)
                .HasMaxLength(20);

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.CreatedBy)
                .HasDefaultValue(Guid.Empty);

            builder.HasIndex(x => x.UserId)
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_CustomerAddresses_UserId");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}