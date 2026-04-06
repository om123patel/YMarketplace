using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence.Configurations
{
    public class CommissionRuleConfiguration
        : IEntityTypeConfiguration<CommissionRule>
    {
        public void Configure(EntityTypeBuilder<CommissionRule> b)
        {
            b.ToTable("CommissionRules", "payments");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn();

            b.Property(x => x.Name)
             .IsRequired().HasMaxLength(100);

            b.Property(x => x.RatePercent)
             .HasColumnType("decimal(5,2)").IsRequired();

            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.IsDeleted).HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.Property(x => x.CreatedBy).HasDefaultValue(Guid.Empty);

            // One rule per category (null = global default — only one allowed)
            b.HasIndex(x => x.CategoryId)
             .IsUnique()
             .HasFilter("[IsDeleted] = 0");

            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}