using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Infrastructure.Persistence.Configurations
{
    public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
    {
        public void Configure(EntityTypeBuilder<Dispute> builder)
        {
            builder.ToTable("Disputes", "orders");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.BuyerEvidence).HasMaxLength(2000);
            builder.Property(x => x.SellerResponse).HasMaxLength(2000);
            builder.Property(x => x.AdminNote).HasMaxLength(2000);
            builder.Property(x => x.Resolution).HasMaxLength(2000);

            builder.Property(x => x.Status)
                .HasConversion(s => s.ToString(), s => Enum.Parse<DisputeStatus>(s))
                .HasMaxLength(20).HasDefaultValue(DisputeStatus.Open);

            builder.Property(x => x.IsDeleted).HasDefaultValue(false);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => x.OrderId);
            builder.HasIndex(x => x.Status);
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }

}
