using Microsoft.EntityFrameworkCore;
using Orders.Application.DTOs.Orders;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence;

namespace Orders.Infrastructure.Persistence.Repositories
{
    public class OrderRepository
        : BaseRepository<Order, Guid, OrdersDbContext>, IOrderRepository
    {
        public OrderRepository(OrdersDbContext context) : base(context) { }

        public async Task<Order?> GetByIdWithDetailsAsync(
            Guid id, CancellationToken ct = default)
            => await DbSet
                .Include(o => o.Items)
                .Include(o => o.Disputes)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task<Order?> GetByOrderNumberAsync(
            string orderNumber, CancellationToken ct = default)
            => await DbSet
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

        public async Task<PagedList<Order>> GetPagedAsync(
            OrderFilterRequest filter, CancellationToken ct = default)
        {
            var query = DbSet.Include(o => o.Items).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(o => o.OrderNumber.Contains(filter.Search));

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                var status = Enum.Parse<OrderStatus>(filter.Status);
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(filter.PaymentStatus))
            {
                var ps = Enum.Parse<PaymentStatus>(filter.PaymentStatus);
                query = query.Where(o => o.PaymentStatus == ps);
            }

            if (filter.BuyerId.HasValue)
                query = query.Where(o => o.BuyerId == filter.BuyerId.Value);

            if (filter.StoreId.HasValue)
                query = query.Where(o => o.StoreId == filter.StoreId.Value);

            if (filter.SellerId.HasValue)
                query = query.Where(o => o.SellerId == filter.SellerId.Value);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.CreatedTo.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "total" => filter.SortDirection == "asc"
                    ? query.OrderBy(o => o.TotalAmount)
                    : query.OrderByDescending(o => o.TotalAmount),
                "status" => filter.SortDirection == "asc"
                    ? query.OrderBy(o => o.Status)
                    : query.OrderByDescending(o => o.Status),
                _ => query.OrderByDescending(o => o.CreatedAt)
            };

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            return new PagedList<Order>(items, filter.Page, filter.PageSize, total);
        }

        public async Task<PagedList<Order>> GetByBuyerIdAsync(
            Guid buyerId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = DbSet.Where(o => o.BuyerId == buyerId)
                .OrderByDescending(o => o.CreatedAt);

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedList<Order>(items, page, pageSize, total);
        }

        public async Task<PagedList<Order>> GetByStoreIdAsync(
            Guid storeId, int page, int pageSize,
            string? status, CancellationToken ct = default)
        {
            var query = DbSet.Where(o => o.StoreId == storeId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusEnum = Enum.Parse<OrderStatus>(status);
                query = query.Where(o => o.Status == statusEnum);
            }

            query = query.OrderByDescending(o => o.CreatedAt);
            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedList<Order>(items, page, pageSize, total);
        }
    }

}
