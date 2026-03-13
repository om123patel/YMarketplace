using Shared.Application.Interfaces;

namespace Identity.Infrastructure.Persistence.UnitOfWork
{
    public class IdentityUnitOfWork : IUnitOfWork
    {
        private readonly IdentityDbContext _context;

        public IdentityUnitOfWork(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }

}
