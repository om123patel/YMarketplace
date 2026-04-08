using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence;

namespace Inventory.Infrastructure.Persistence.Repositories
{
    public class StockRepository
        : BaseRepository<Stock, Guid, InventoryDbContext>, IStockRepository
    {
        public StockRepository(InventoryDbContext context) : base(context) { }

        public async Task<Stock?> GetByProductIdAsync(
            Guid productId, CancellationToken ct = default)
            => await DbSet
                .Include(s => s.Reservations.Where(
                    r => r.Status == ReservationStatus.Active))
                .FirstOrDefaultAsync(s => s.ProductId == productId, ct);

        public async Task<Stock?> GetByProductAndVariantAsync(
            Guid productId, Guid? variantId, CancellationToken ct = default)
            => await DbSet
                .Include(s => s.Reservations.Where(
                    r => r.Status == ReservationStatus.Active))
                .FirstOrDefaultAsync(s =>
                    s.ProductId == productId &&
                    s.VariantId == variantId, ct);

        public async Task<bool> ExistsForProductAsync(
            Guid productId, CancellationToken ct = default)
            => await DbSet.AnyAsync(s => s.ProductId == productId, ct);

        public async Task<IEnumerable<Stock>> GetLowStockAsync(
            CancellationToken ct = default)
            => await DbSet
                .Where(s =>
                    s.TrackInventory &&
                    s.Status == StockStatus.LowStock)
                .OrderBy(s => s.Quantity)
                .ToListAsync(ct);

        public async Task<PagedList<Stock>> GetPagedAsync(
            StockFilterRequest filter, CancellationToken ct = default)
        {
            var query = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(s =>
                    s.ProductId.ToString().Contains(filter.Search));

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                var status = Enum.Parse<StockStatus>(filter.Status);
                query = query.Where(s => s.Status == status);
            }

            if (filter.LowStockOnly == true)
                query = query.Where(s => s.Status == StockStatus.LowStock);

            query = filter.SortBy?.ToLower() switch
            {
                "quantity" => filter.SortDirection == "desc"
                    ? query.OrderByDescending(s => s.Quantity)
                    : query.OrderBy(s => s.Quantity),
                _ => query.OrderByDescending(s => s.UpdatedAt)
            };

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            return new PagedList<Stock>(items, filter.Page, filter.PageSize, total);
        }
    }
}