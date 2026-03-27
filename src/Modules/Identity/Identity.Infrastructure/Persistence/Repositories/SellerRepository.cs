using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using System.Data;
using System.Globalization;

namespace Identity.Infrastructure.Persistence.Repositories
{
    public class SellerRepository : ISellerRepository
    {
        private readonly IdentityDbContext _db;

        public SellerRepository(IdentityDbContext db) => _db = db;

        public async Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Sellers
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<Seller?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Sellers
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken);
        }

        public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Sellers
                .AnyAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken);
        }

        public async Task AddAsync(Seller seller, CancellationToken cancellationToken = default)
        {
            await _db.Sellers.AddAsync(seller, cancellationToken);
        }

        public void Update(Seller seller)
        {
            _db.Sellers.Update(seller);
        }

        public async Task<IReadOnlyList<Seller>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Sellers
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedList<SellerDto>> GetPagedAsync(int page, int pageSize, string? status,
    string? search, string ? sortBy = null, string? sortDirection = null,
    CancellationToken ct = default)
        {
            var query = _db.Sellers
                .Include(s => s.User) // 🔥 REQUIRED
                .Where(s => !s.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<SellerStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(s => s.Status == statusEnum);
                }
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.BusinessName.Contains(search) ||
                    x.User.FirstName.Contains(search) ||
                    x.User.LastName.Contains(search) ||
                    x.User.Email.Contains(search));
            }

            var total = await query.CountAsync(ct);

            query = sortBy switch
            {
                "businessname" => sortDirection == "asc"
                    ? query.OrderBy(s => s.BusinessName)
                    : query.OrderByDescending(s => s.BusinessName),

                _ => query.OrderByDescending(s => s.CreatedAt)
            };

            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SellerDto
                {
                    Id = s.Id,
                    UserId = s.UserId,

                    FullName = s.User.FullName,
                    Email = s.User.Email,
                    AvatarUrl = s.User.AvatarUrl,

                    BusinessName = s.BusinessName,
                    SellerStatus = s.Status.ToString(),

                    City = s.Address.City,
                    Country = s.Address.Country,

                    TotalProducts = s.TotalProducts,
                    TotalOrders = s.TotalOrders,
                    TotalRevenue = s.TotalRevenue,
                    Rating = s.Rating,

                    CreatedAt = s.CreatedAt
                })
                .ToListAsync(ct);

            return new PagedList<SellerDto>(items, page, pageSize, total);
        }
    }
}
