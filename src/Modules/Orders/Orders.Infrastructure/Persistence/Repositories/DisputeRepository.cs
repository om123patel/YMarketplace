using Microsoft.EntityFrameworkCore;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence;

namespace Orders.Infrastructure.Persistence.Repositories
{
    public class DisputeRepository
         : BaseRepository<Dispute, Guid, OrdersDbContext>, IDisputeRepository
    {
        public DisputeRepository(OrdersDbContext context) : base(context) { }

        public async Task<PagedList<Dispute>> GetPagedAsync(
            int page, int pageSize, string? status, CancellationToken ct = default)
        {
            var query = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusEnum = Enum.Parse<DisputeStatus>(status);
                query = query.Where(d => d.Status == statusEnum);
            }

            query = query.OrderByDescending(d => d.CreatedAt);
            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedList<Dispute>(items, page, pageSize, total);
        }

        public async Task<Dispute?> GetByOrderIdAsync(
            Guid orderId, CancellationToken ct = default)
            => await DbSet.FirstOrDefaultAsync(d => d.OrderId == orderId, ct);
    }

}
