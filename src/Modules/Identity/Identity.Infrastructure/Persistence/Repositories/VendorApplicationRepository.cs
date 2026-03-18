using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;

namespace Identity.Infrastructure.Persistence.Repositories
{
    public class VendorApplicationRepository : IVendorApplicationRepository
    {
        private readonly IdentityDbContext _db;

        public VendorApplicationRepository(IdentityDbContext db) => _db = db;

        public async Task<VendorApplication?> GetByIdAsync(
            Guid id, CancellationToken ct = default)
            => await _db.VendorApplications
                .FirstOrDefaultAsync(a => a.Id == id, ct);

        public async Task<VendorApplication?> GetByUserIdAsync(
            Guid userId, CancellationToken ct = default)
            => await _db.VendorApplications
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(ct);

        public async Task<PagedList<VendorApplication>> GetPagedAsync(
            int page, int pageSize,
            VendorApplicationStatus? status,
            CancellationToken ct = default)
        {
            var query = _db.VendorApplications.AsQueryable();

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedList<VendorApplication>(items, page, pageSize, total);
        }

        public async Task AddAsync(
            VendorApplication app, CancellationToken ct = default)
            => await _db.VendorApplications.AddAsync(app, ct);

        public void Update(VendorApplication app)
            => _db.VendorApplications.Update(app);
    }

}
