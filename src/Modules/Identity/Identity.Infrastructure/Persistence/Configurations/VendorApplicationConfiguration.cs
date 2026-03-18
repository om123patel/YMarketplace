using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations
{
    public class VendorApplicationConfiguration
        : IEntityTypeConfiguration<VendorApplication>
    {
        public void Configure(EntityTypeBuilder<VendorApplication> b)
        {
            b.ToTable("VendorApplications", "identity");
            b.HasKey(a => a.Id);

            b.Property(a => a.StoreName).IsRequired().HasMaxLength(200);
            b.Property(a => a.BusinessType).IsRequired().HasMaxLength(100);
            b.Property(a => a.TaxId).HasMaxLength(50);
            b.Property(a => a.ContactPhone).HasMaxLength(20);
            b.Property(a => a.Description).HasMaxLength(2000);
            b.Property(a => a.RejectionReason).HasMaxLength(1000);

            b.Property(a => a.Status)
                .HasConversion(
                    s => s.ToString(),
                    s => Enum.Parse<VendorApplicationStatus>(s))
                .HasMaxLength(20)
                .IsRequired();

            b.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_VendorApplications_UserId");

            b.HasIndex(a => a.Status)
                .HasDatabaseName("IX_VendorApplications_Status");
        }
    }

}
