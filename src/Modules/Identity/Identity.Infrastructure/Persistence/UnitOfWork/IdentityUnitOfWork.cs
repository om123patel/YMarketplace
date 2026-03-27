using Identity.Application.Interfaces;
using Shared.Application.Interfaces;

namespace Identity.Infrastructure.Persistence.UnitOfWork
{
    public class IdentityUnitOfWork : IIdentityUnitOfWork
    {
        private readonly IdentityDbContext _context;

        public IdentityUnitOfWork(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

    }

}
