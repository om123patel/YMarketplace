using Payments.Application.Interfaces;

namespace Payments.Infrastructure.Persistence.UnitOfWork
{
    public class PaymentsUnitOfWork : IPaymentsUnitOfWork
    {
        private readonly PaymentsDbContext _context;

        public PaymentsUnitOfWork(PaymentsDbContext context)
            => _context = context;

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}