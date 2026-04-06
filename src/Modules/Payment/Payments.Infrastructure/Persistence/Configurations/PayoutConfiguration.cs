using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;
using Payments.Domain.Enums;

namespace Payments.Infrastructure.Persistence.Configurations
{
    public class PayoutConfiguration
        : IEntityTypeConfiguration<Payout>
    {
        public void Configure(EntityTypeBuilder<Payout> b)
        {
            b.ToTable("Payouts", "payments");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
             .HasDefaultValueSql("NEWSEQUENTIALID()");

            b.Property(x => x.Amount)
             .HasColumnType("decimal(18,4)").IsRequired();

            b.Property(x => x.CurrencyCode)
             .HasMaxLength(3).HasDefaultValue("INR");

            b.Property(x => x.Status)
             .HasConversion(
                 s => s.ToString(),
                 s => Enum.Parse<PayoutStatus>(s))
             .HasMaxLength(20)
             .HasDefaultValue(PayoutStatus.Pending);

            b.Property(x => x.BankAccountNumber).HasMaxLength(20);
            b.Property(x => x.BankIfscCode).HasMaxLength(15);
            b.Property(x => x.BankAccountName).HasMaxLength(150);
            b.Property(x => x.UpiId).HasMaxLength(100);
            b.Property(x => x.AdminNote).HasMaxLength(500);
            b.Property(x => x.FailureReason).HasMaxLength(500);
            b.Property(x => x.GatewayReference).HasMaxLength(200);

            b.Property(x => x.IsDeleted).HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.Property(x => x.CreatedBy).HasDefaultValue(Guid.Empty);

            b.Property(x => x.RowVersion).IsRowVersion();

            b.HasMany(x => x.PayoutTransactions)
             .WithOne()
             .HasForeignKey(x => x.PayoutId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.SellerId)
             .HasFilter("[IsDeleted] = 0");

            b.HasIndex(x => x.Status)
             .HasFilter("[IsDeleted] = 0");

            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}