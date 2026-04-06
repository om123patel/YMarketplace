using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence.Configurations
{
    public class PayoutTransactionConfiguration
        : IEntityTypeConfiguration<PayoutTransaction>
    {
        public void Configure(EntityTypeBuilder<PayoutTransaction> b)
        {
            b.ToTable("PayoutTransactions", "payments");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn();

            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.Property(x => x.CreatedBy).HasDefaultValue(Guid.Empty);

            b.HasIndex(x => new { x.PayoutId, x.TransactionId })
             .IsUnique();
        }
    }
}