using Catalog.Domain.Enums;
using Catalog.Domain.Events;
using Catalog.Domain.Exceptions;
using Shared.Domain.Abstractions;
using Shared.Domain.Exceptions;
using Shared.Domain.Primitives;

namespace Catalog.Domain.Entities
{
    public class Product : AggregateRoot<Guid>, IConcurrencyToken
    {
        // Ownership
        public Guid? SellerId { get; private set; }
        public Guid? StoreId { get; private set; }
        public Guid? CreatedByAdminId { get; private set; }
        public ProductCreatorType CreatorType { get; private set; }

        // Add this property
        public byte[]? RowVersion { get; private set; }

        // Classification
        public int CategoryId { get; private set; }
        public int? BrandId { get; private set; }

        // Core
        public string Name { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public string? ShortDescription { get; private set; }
        public string? Description { get; private set; }

        // Pricing
        public Money BasePrice { get; private set; } = default!;
        public Money? CompareAtPrice { get; private set; }
        public Money? CostPrice { get; private set; }

        // Identifiers
        public string? Sku { get; private set; }
        public string? Barcode { get; private set; }

        // Physical
        public decimal? WeightKg { get; private set; }
        public decimal? LengthCm { get; private set; }
        public decimal? WidthCm { get; private set; }
        public decimal? HeightCm { get; private set; }

        // Flags
        public bool IsDigital { get; private set; }
        public bool RequiresShipping { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsFeatured { get; private set; }

        // Approval workflow
        public ProductStatus Status { get; private set; }
        public DateTime? SubmittedForApprovalAt { get; private set; }
        public Guid? ApprovedByAdminId { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public Guid? RejectedByAdminId { get; private set; }
        public DateTime? RejectedAt { get; private set; }
        public string? RejectionReason { get; private set; }

        // Navigation
        public Category Category { get; private set; } = default!;
        public Brand? Brand { get; private set; }
        public ProductSeo Seo { get; private set; } = default!;
        public ICollection<ProductImage> Images { get; private set; } = [];
        public ICollection<ProductVariant> Variants { get; private set; } = [];
        public ICollection<Tag> Tags { get; private set; } = [];

        

        private Product() { }  // EF Core

        // ── Factory: Seller creates product ──
        public static Product CreateBySeller(
            Guid sellerId,
            Guid storeId,
            int categoryId,
            string name,
            string slug,
            Money basePrice,
            Guid createdBy,
            int? brandId = null,
            string? shortDescription = null,
            string? description = null,
            string? sku = null,
            bool isDigital = false)
        {
            var product = CreateCore(
                categoryId, name, slug, basePrice, createdBy,
                brandId, shortDescription, description, sku, isDigital);

            product.SellerId = sellerId;
            product.StoreId = storeId;
            product.CreatorType = ProductCreatorType.Seller;

            product.RaiseDomainEvent(
                new ProductCreatedEvent(product.Id, product.Name, sellerId));

            return product;
        }

        // ── Factory: Admin creates product directly ──
        public static Product CreateByAdmin(
            Guid adminId,
            int categoryId,
            string name,
            string slug,
            Money basePrice,
            Guid createdBy,
            int? brandId = null,
            string? shortDescription = null,
            string? description = null,
            string? sku = null,
            bool isDigital = false,
            Guid? sellerId = null,     // optional — admin can create on behalf of seller
            Guid? storeId = null)
        {
            var product = CreateCore(
                categoryId, name, slug, basePrice, createdBy,
                brandId, shortDescription, description, sku, isDigital);

            product.CreatedByAdminId = adminId;
            product.CreatorType = ProductCreatorType.Admin;
            product.SellerId = sellerId;
            product.StoreId = storeId;

            // Admin-created products skip PendingApproval — go straight to Active
            product.Status = ProductStatus.Active;
            product.IsActive = true;
            product.ApprovedByAdminId = adminId;
            product.ApprovedAt = DateTime.UtcNow;

            product.RaiseDomainEvent(
                new ProductCreatedEvent(product.Id, product.Name, sellerId));

            return product;
        }

        // ── Shared internal factory ──
        private static Product CreateCore(
            int categoryId,
            string name,
            string slug,
            Money basePrice,
            Guid createdBy,
            int? brandId,
            string? shortDescription,
            string? description,
            string? sku,
            bool isDigital)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                BrandId = brandId,
                Name = name,
                Slug = slug.ToLowerInvariant(),
                ShortDescription = shortDescription,
                Description = description,
                BasePrice = basePrice,
                Sku = sku,
                IsDigital = isDigital,
                RequiresShipping = !isDigital,
                IsActive = false,
                IsFeatured = false,
                Status = ProductStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            // Always create an SEO record alongside the product
            product.Seo = ProductSeo.Create(product.Id);

            return product;
        }

        // ── Status transitions ──
        public void SendForReapproval(Guid sellerId)
        {
            if (SellerId != sellerId)
                throw new DomainException(
                    "UNAUTHORIZED",
                    "You can only resubmit your own products.");

            Status = ProductStatus.PendingApproval;
            IsActive = false;
            SetUpdatedBy(sellerId);

            RaiseDomainEvent(new ProductStatusChangedEvent(
                Id,
                ProductStatus.Active,
                ProductStatus.PendingApproval,
                sellerId));  // ← ADD
        }


        public void ResubmitForApproval(Guid sellerId)
        {
            if (CreatorType != ProductCreatorType.Seller)
                throw new DomainException(
                    "INVALID_OPERATION",
                    "Only seller products can be resubmitted for approval.");

            if (SellerId != sellerId)
                throw new DomainException(
                    "UNAUTHORIZED",
                    "You can only resubmit your own products.");

            if (Status != ProductStatus.Rejected)
                throw new DomainException(
                    "INVALID_STATUS_TRANSITION",
                    "Only rejected products can be resubmitted for approval.");

            Status = ProductStatus.PendingApproval;
            IsActive = false;
            SetUpdatedBy(sellerId);

            RaiseDomainEvent(new ProductStatusChangedEvent(
                Id,
                ProductStatus.Rejected,
                ProductStatus.PendingApproval,
                sellerId));  // ← ADD
        }


        public void Approve(Guid adminId)
        {
            if (Status != ProductStatus.PendingApproval)
                throw new DomainException(
                    "INVALID_STATUS_TRANSITION",
                    "Only products pending approval can be approved.");

            Status = ProductStatus.Active;
            IsActive = true;
            SetUpdatedBy(adminId);

            RaiseDomainEvent(new ProductStatusChangedEvent(
                Id,
                ProductStatus.PendingApproval,
                ProductStatus.Active,
                adminId));  // ← ADD
        }


        public void Reject(Guid adminId, string reason)
        {
            if (Status != ProductStatus.PendingApproval)
                throw new DomainException(
                    "INVALID_STATUS_TRANSITION",
                    "Only products pending approval can be rejected.");

            Status = ProductStatus.Rejected;
            IsActive = false;
            RejectionReason = reason;
            SetUpdatedBy(adminId);

            RaiseDomainEvent(new ProductStatusChangedEvent(
                Id,
                ProductStatus.PendingApproval,
                ProductStatus.Rejected,
                adminId));  // ← ADD
        }


        public void Archive(Guid userId)
        {
            if (Status == ProductStatus.Archived)
                throw new DomainException(
                    "INVALID_STATUS_TRANSITION",
                    "Product is already archived.");

            var previousStatus = Status;
            Status = ProductStatus.Archived;
            IsActive = false;
            SetUpdatedBy(userId);

            RaiseDomainEvent(new ProductStatusChangedEvent(
                Id,
                previousStatus,
                ProductStatus.Archived,
                userId));  // ← ADD
        }


        public void Feature(Guid adminId) { IsFeatured = true; SetUpdatedBy(adminId); }
        public void Unfeature(Guid adminId) { IsFeatured = false; SetUpdatedBy(adminId); }

        // ── Child management ──
        public void AddImage(string imageUrl, Guid createdBy,
            string? thumbnailUrl = null, string? altText = null,
            int sortOrder = 0, bool isPrimary = false)
        {
            if (isPrimary)
                foreach (var img in Images) img.UnsetAsPrimary();

            var image = ProductImage.Create(
                Id, imageUrl, createdBy, thumbnailUrl, altText, sortOrder, isPrimary);

            Images.Add(image);
            SetUpdatedBy(createdBy);
        }

        public ProductVariant AddVariant(string name, Money price, Guid createdBy,
            string? sku = null, Money? compareAtPrice = null,
            decimal? weightKg = null, string? imageUrl = null, int sortOrder = 0)
        {
            var variant = ProductVariant.Create(
                Id, name, price, createdBy, sku,
                compareAtPrice: compareAtPrice,
                weightKg: weightKg, imageUrl: imageUrl, sortOrder: sortOrder);

            Variants.Add(variant);
            SetUpdatedBy(createdBy);
            return variant;
        }

        public void UpdateSeo(Guid updatedBy,
            string? metaTitle = null, string? metaDescription = null,
            string? metaKeywords = null, string? canonicalUrl = null,
            string? ogTitle = null, string? ogDescription = null,
            string? ogImageUrl = null)
        {
            Seo.Update(updatedBy, metaTitle, metaDescription,
                metaKeywords, canonicalUrl, ogTitle, ogDescription, ogImageUrl);
            SetUpdatedBy(updatedBy);
        }

        public void UpdateDetails(
            string name, string slug, Money basePrice, Guid updatedBy,
            int categoryId, int? brandId = null,
            string? shortDescription = null, string? description = null,
            string? sku = null, decimal? weightKg = null)
        {
            Name = name;
            Slug = slug.ToLowerInvariant();
            BasePrice = basePrice;
            CategoryId = categoryId;
            BrandId = brandId;
            ShortDescription = shortDescription;
            Description = description;
            Sku = sku;
            WeightKg = weightKg;
            SetUpdatedBy(updatedBy);
        }


       

    }

}
