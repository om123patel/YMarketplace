using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using System.Reflection;

namespace Payments.Infrastructure.Persistence
{
    public class PaymentsDbContext : DbContext
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
            : base(options) { }

        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();
        public DbSet<Payout> Payouts => Set<Payout>();
        public DbSet<PayoutTransaction> PayoutTransactions => Set<PayoutTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("payments");
            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}