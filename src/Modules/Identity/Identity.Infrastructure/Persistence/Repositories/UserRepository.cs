using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Models;
using Shared.Infrastructure.Persistence;

namespace Identity.Infrastructure.Persistence.Repositories
{
    public class UserRepository : BaseRepository<User, Guid, IdentityDbContext>,  IUserRepository
    {
        
        public UserRepository(IdentityDbContext db) : base(db) { }

        public async Task<User?> GetByEmailAsync(
            string email, CancellationToken ct = default)
            => await DbSet.FirstOrDefaultAsync(
                u => u.Email == email.ToLowerInvariant(), ct);

        public async Task<bool> ExistsByEmailAsync(
            string email, CancellationToken ct = default)
            => await DbSet.AnyAsync(
                u => u.Email == email.ToLowerInvariant(), ct);

        public async Task<PagedList<User>> GetPagedAsync(
            int page, int pageSize,
            string? role, string? status, string? search,
            CancellationToken ct = default)
        {
            var query = DbSet.AsQueryable();

         


            if (!string.IsNullOrWhiteSpace(role))
            {
                // 1. Convert the input string to the Enum type first
                if (Enum.TryParse<UserRole>(role, true, out var roleEnum))
                {
                    // 2. Filter using the typed enum value
                    query = query.Where(u => u.Role == roleEnum);
                }
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                // 1. Convert the input string to the Enum type first
                if (Enum.TryParse<UserStatus>(status, true, out var statusEnum))
                {
                    // 2. Filter using the typed enum value
                    query = query.Where(u => u.Status == statusEnum);
                }
            }

            

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.Email.Contains(search) ||
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search));

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedList<User>(items, page, pageSize, total);
        }

        
    }


}
