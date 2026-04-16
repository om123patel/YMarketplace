// src/Modules/Identity/Identity.Infrastructure/Persistence/Repositories/CustomerAddressRepository.cs

using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;

namespace Identity.Infrastructure.Persistence.Repositories
{
    public class CustomerAddressRepository
        : BaseRepository<CustomerAddress, Guid, IdentityDbContext>,
          ICustomerAddressRepository
    {
        public CustomerAddressRepository(IdentityDbContext db) : base(db) { }

        public async Task<IEnumerable<CustomerAddress>> GetByUserIdAsync(
            Guid userId, CancellationToken ct = default)
            => await DbSet
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync(ct);

        public async Task<CustomerAddress?> GetDefaultAsync(
            Guid userId, CancellationToken ct = default)
            => await DbSet
                .FirstOrDefaultAsync(
                    a => a.UserId == userId && a.IsDefault, ct);
    }
}