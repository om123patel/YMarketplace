using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;

namespace Shared.Infrastructure.Persistence
{
    public abstract class BaseRepository<TEntity, TId, TContext>
    : IRepository<TEntity, TId>
    where TEntity : class
    where TContext : DbContext
    {
        protected readonly TContext Context;
        protected readonly DbSet<TEntity> DbSet;

        protected BaseRepository(TContext context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(
            TId id,
            CancellationToken cancellationToken = default)
            => await DbSet.FindAsync([id], cancellationToken);

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => await DbSet.ToListAsync(cancellationToken);

        public virtual async Task AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
            => await DbSet.AddAsync(entity, cancellationToken);

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);           
        }
        

        public virtual void Remove(TEntity entity)
            => DbSet.Remove(entity);
    }
}
