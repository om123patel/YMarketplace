using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Catalog.Infrastructure.Persistence
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductSeo> ProductSeos => Set<ProductSeo>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
        public DbSet<AttributeTemplate> AttributeTemplates => Set<AttributeTemplate>();
        public DbSet<AttributeTemplateItem> AttributeTemplateItems => Set<AttributeTemplateItem>();
        public DbSet<ProductStatusHistory> ProductStatusHistories => Set<ProductStatusHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set default schema
            modelBuilder.HasDefaultSchema("catalog");

            // Apply all configurations from this assembly automatically
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }

}
