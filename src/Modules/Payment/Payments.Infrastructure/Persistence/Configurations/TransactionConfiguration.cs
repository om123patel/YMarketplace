using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;
using Payments.Domain.Enums;

namespace Payments.Infrastructure.Persistence.Configurations
{
    public class TransactionConfiguration
        : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> b)
        {
            b.ToTable("Transactions", "payments");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id)
             .HasDefaultValueSql("NEWSEQUENTIALID()");

            b.Property(x => x.Amount)
             .HasColumnType("decimal(18,4)").IsRequired();

            b.Property(x => x.CommissionAmount)
             .HasColumnType("decimal(18,4)").IsRequired();

            b.Property(x => x.SellerAmount)
             .HasColumnType("decimal(18,4)").IsRequired();

            b.Property(x => x.RefundedAmount)
             .HasColumnType("decimal(18,4)")
             .HasDefaultValue(0m);

            b.Property(x => x.CurrencyCode)
             .HasMaxLength(3).IsRequired()
             .HasDefaultValue("INR");

            b.Property(x => x.Status)
             .HasConversion(
                 s => s.ToString(),
                 s => Enum.Parse<TransactionStatus>(s))
             .HasMaxLength(30)
             .HasDefaultValue(TransactionStatus.Pending);

            b.Property(x => x.Method)
             .HasConversion(
                 m => m.ToString(),
                 m => Enum.Parse<PaymentMethod>(m))
             .HasMaxLength(20);

            b.Property(x => x.GatewayTransactionId).HasMaxLength(200);
            b.Property(x => x.GatewayProvider).HasMaxLength(50);
            b.Property(x => x.GatewayResponse).HasColumnType("nvarchar(max)");
            b.Property(x => x.RefundReason).HasMaxLength(500);

            b.Property(x => x.IsDeleted).HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.Property(x => x.CreatedBy)
             .HasDefaultValue(Guid.Empty);

            b.Property(x => x.GatewayOrderId).HasMaxLength(100);
            b.Property(x => x.GatewayCheckoutUrl).HasMaxLength(500);

            b.HasIndex(x => x.GatewayOrderId)
             .HasFilter("[GatewayOrderId] IS NOT NULL");


            b.Property(x => x.RowVersion).IsRowVersion();

            b.HasIndex(x => x.OrderId)
             .IsUnique()
             .HasFilter("[IsDeleted] = 0");

            b.HasIndex(x => x.SellerId)
             .HasFilter("[IsDeleted] = 0");

            b.HasIndex(x => x.BuyerId)
             .HasFilter("[IsDeleted] = 0");

            b.HasIndex(x => x.Status)
             .HasFilter("[IsDeleted] = 0");

            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}