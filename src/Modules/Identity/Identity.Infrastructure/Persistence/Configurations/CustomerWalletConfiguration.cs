// src/Modules/Identity/Identity.Infrastructure/Persistence/Configurations/CustomerWalletConfiguration.cs

using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations
{
    public class CustomerWalletConfiguration
        : IEntityTypeConfiguration<CustomerWallet>
    {
        public void Configure(EntityTypeBuilder<CustomerWallet> builder)
        {
            builder.ToTable("CustomerWallets", "identity");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Balance)
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0m);

            builder.Property(x => x.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("INR");

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.CreatedBy)
                .HasDefaultValue(Guid.Empty);

            builder.HasMany(x => x.Transactions)
                .WithOne()
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.UserId)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UIX_Wallets_UserId");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }

    public class WalletTransactionConfiguration
        : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.ToTable("WalletTransactions", "identity");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(x => x.Type)
                .HasConversion(
                    t => t.ToString(),
                    t => Enum.Parse<WalletTransactionType>(t))
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.ReferenceId)
                .HasMaxLength(200);

            builder.Property(x => x.BalanceAfter)
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.CreatedBy)
                .HasDefaultValue(Guid.Empty);

            builder.HasIndex(x => x.WalletId)
                .HasDatabaseName("IX_WalletTransactions_WalletId");

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}