using Orders.Application.Interfaces;

namespace Orders.Infrastructure.Persistence.UnitOfWork
{
    public class OrdersUnitOfWork : IOrdersUnitOfWork
    {
        private readonly OrdersDbContext _context;

        public OrdersUnitOfWork(OrdersDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }

}
