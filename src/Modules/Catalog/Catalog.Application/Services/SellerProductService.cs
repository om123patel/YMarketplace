using AutoMapper;
using Catalog.Application.DTOs.Products;
using Catalog.Application.Interfaces;
using Catalog.Application.Services.Interface;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using FluentValidation;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Exceptions;
using Shared.Domain.Primitives;

namespace Catalog.Application.Services
{
    public class SellerProductService : ISellerProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IProductStatusHistoryRepository _statusHistoryRepository;
        private readonly IStorageService _storageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateSellerProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;

        public SellerProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            ITagRepository tagRepository,
            IProductStatusHistoryRepository statusHistoryRepository,
            IStorageService storageService,
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            IMapper mapper,
            IValidator<CreateSellerProductDto> createValidator,
            IValidator<UpdateProductDto> updateValidator)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _tagRepository = tagRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _storageService = storageService;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // ══════════════════════════════════════════════════════
        // QUERIES — always scoped to sellerId
        // ══════════════════════════════════════════════════════

        public async Task<Result<ProductDto>> GetByIdAsync(
            Guid id, Guid sellerId, CancellationToken ct = default)
        {
            var product = await _productRepository
                .GetByIdWithDetailsAsync(id, ct);

            if (product is null || product.IsDeleted)
                return Result<ProductDto>.Failure(
                    $"Product {id} not found.", "PRODUCT_NOT_FOUND");

            // Seller can only see their own products
            if (product.SellerId != sellerId)
                return Result<ProductDto>.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
        }

        public async Task<Result<PagedList<ProductListItemDto>>> GetPagedAsync(
            SellerProductFilterRequest filter,
            Guid sellerId,
            CancellationToken ct = default)
        {
            // Convert to ProductFilterRequest with sellerId locked
            var adminFilter = new ProductFilterRequest
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                Search = filter.Search,
                Status = filter.Status,
                CategoryId = filter.CategoryId,
                BrandId = filter.BrandId,
                IsFeatured = filter.IsFeatured,
                CreatedFrom = filter.CreatedFrom,
                CreatedTo = filter.CreatedTo,
                SortBy = filter.SortBy,
                SortDirection = filter.SortDirection,
                SellerId = sellerId   // ← always locked to this seller
            };

            var paged = await _productRepository.GetPagedAsync(adminFilter, ct);

            var mapped = new PagedList<ProductListItemDto>(
                _mapper.Map<List<ProductListItemDto>>(paged.Items),
                paged.Page,
                paged.PageSize,
                paged.TotalCount);

            return Result<PagedList<ProductListItemDto>>.Success(mapped);
        }

        // ══════════════════════════════════════════════════════
        // CREATE — goes to PendingApproval
        // ══════════════════════════════════════════════════════

        public async Task<Result<ProductDto>> CreateAsync(
            CreateSellerProductDto dto,
            Guid sellerId,
            Guid storeId,
            CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<ProductDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Check category
            if (!await _categoryRepository.ExistsAsync(dto.CategoryId, ct))
                return Result<ProductDto>.Failure(
                    $"Category {dto.CategoryId} not found.", "CATEGORY_NOT_FOUND");

            // 3. Check brand
            if (dto.BrandId.HasValue &&
                !await _brandRepository.ExistsAsync(dto.BrandId.Value, ct))
                return Result<ProductDto>.Failure(
                    $"Brand {dto.BrandId} not found.", "BRAND_NOT_FOUND");

            // 4. Check slug uniqueness
            if (await _productRepository.SlugExistsAsync(dto.Slug, ct))
                return Result<ProductDto>.Failure(
                    $"Slug '{dto.Slug}' is already taken.", "SLUG_EXISTS");

            // 5. Check SKU uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Sku) &&
                await _productRepository.SkuExistsAsync(dto.Sku, ct))
                return Result<ProductDto>.Failure(
                    $"SKU '{dto.Sku}' already exists.", "SKU_EXISTS");

            // 6. Validate tags
            if (dto.TagIds.Count > 0)
            {
                var tags = await _tagRepository.GetByIdsAsync(dto.TagIds, ct);
                var missingIds = dto.TagIds
                    .Except(tags.Select(t => t.Id)).ToList();

                if (missingIds.Count > 0)
                    return Result<ProductDto>.Failure(
                        $"Tags not found: {string.Join(", ", missingIds)}",
                        "TAGS_NOT_FOUND");
            }

            // 7. Build Money
            var basePrice = new Money(dto.BasePrice, dto.CurrencyCode);
            var compareAtPrice = dto.CompareAtPrice.HasValue
                ? new Money(dto.CompareAtPrice.Value, dto.CurrencyCode)
                : null;

            // 8. Create via SELLER factory — status = PendingApproval
            var product = Product.CreateBySeller(
                sellerId: sellerId,
                storeId: storeId,
                categoryId: dto.CategoryId,
                name: dto.Name,
                slug: dto.Slug,
                basePrice: basePrice,
                createdBy: sellerId,
                brandId: dto.BrandId,
                shortDescription: dto.ShortDescription,
                description: dto.Description,
                sku: dto.Sku,
                isDigital: dto.IsDigital);

            // 9. Add variants
            foreach (var variantDto in dto.Variants.OrderBy(v => v.SortOrder))
            {
                var vPrice = new Money(variantDto.Price, variantDto.CurrencyCode);
                var vCompare = variantDto.CompareAtPrice.HasValue
                    ? new Money(variantDto.CompareAtPrice.Value, variantDto.CurrencyCode)
                    : null;

                var variant = product.AddVariant(
                    name: variantDto.Name,
                    price: vPrice,
                    createdBy: sellerId,
                    sku: variantDto.Sku,
                    compareAtPrice: vCompare,
                    weightKg: variantDto.WeightKg,
                    imageUrl: variantDto.ImageUrl,
                    sortOrder: variantDto.SortOrder);

                foreach (var attr in variantDto.Attributes.OrderBy(a => a.SortOrder))
                    variant.AddAttribute(attr.Name, attr.Value, attr.SortOrder);
            }

            // 10. Add images
            var isPrimary = true;
            foreach (var imageUrl in dto.ImageUrls)
            {
                product.AddImage(imageUrl, sellerId, isPrimary: isPrimary);
                isPrimary = false;
            }

            // 11. SEO
            if (dto.Seo is not null)
            {
                product.UpdateSeo(
                    sellerId,
                    dto.Seo.MetaTitle,
                    dto.Seo.MetaDescription,
                    dto.Seo.MetaKeywords,
                    dto.Seo.CanonicalUrl,
                    dto.Seo.OgTitle,
                    dto.Seo.OgDescription,
                    dto.Seo.OgImageUrl);
            }

            // 12. Persist product FIRST — FK on ProductStatusHistory
            //     requires the Products row to exist before history is inserted.
            await _productRepository.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // 13. Log status history (product row now committed)
            await _statusHistoryRepository.AddAsync(
                ProductStatusHistory.Create(
                    productId: product.Id,
                    fromStatus: null,
                    toStatus: ProductStatus.PendingApproval,
                    changedBy: sellerId,
                    note: "Product submitted by seller for approval."),
                ct);

            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAndClearEventsAsync(product, ct);

            var created = await _productRepository
                .GetByIdWithDetailsAsync(product.Id, ct);

            return Result<ProductDto>.Success(_mapper.Map<ProductDto>(created!));
        }

        // ══════════════════════════════════════════════════════
        // UPDATE — triggers re-approval if Active/Rejected
        // ══════════════════════════════════════════════════════

        public async Task<Result<ProductDto>> UpdateAsync(
            Guid id,
            UpdateProductDto dto,
            Guid sellerId,
            CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _updateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<ProductDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Find and verify ownership
            var product = await _productRepository
                .GetByIdWithDetailsAsync(id, ct);

            if (product is null || product.IsDeleted)
                return Result<ProductDto>.Failure(
                    $"Product {id} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result<ProductDto>.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            // 3. Cannot edit products that are pending approval
            if (product.Status == ProductStatus.PendingApproval)
                return Result<ProductDto>.Failure(
                    "Cannot edit a product that is pending approval.",
                    "INVALID_STATUS_TRANSITION");

            // 4. Validate category + brand
            if (!await _categoryRepository.ExistsAsync(dto.CategoryId, ct))
                return Result<ProductDto>.Failure(
                    $"Category {dto.CategoryId} not found.", "CATEGORY_NOT_FOUND");

            if (dto.BrandId.HasValue &&
                !await _brandRepository.ExistsAsync(dto.BrandId.Value, ct))
                return Result<ProductDto>.Failure(
                    $"Brand {dto.BrandId} not found.", "BRAND_NOT_FOUND");

            // 5. Check slug + SKU uniqueness
            if (await _productRepository.SlugExistsExceptAsync(dto.Slug, id, ct))
                return Result<ProductDto>.Failure(
                    $"Slug '{dto.Slug}' is already taken.", "SLUG_EXISTS");

            if (!string.IsNullOrWhiteSpace(dto.Sku) &&
                await _productRepository.SkuExistsExceptAsync(dto.Sku, id, ct))
                return Result<ProductDto>.Failure(
                    $"SKU '{dto.Sku}' already exists.", "SKU_EXISTS");

            // 6. Build Money
            var basePrice = new Money(dto.BasePrice, dto.CurrencyCode);

            // 7. Update via domain method
            product.UpdateDetails(
                name: dto.Name,
                slug: dto.Slug,
                basePrice: basePrice,
                updatedBy: sellerId,
                categoryId: dto.CategoryId,
                brandId: dto.BrandId,
                shortDescription: dto.ShortDescription,
                description: dto.Description,
                sku: dto.Sku,
                weightKg: dto.WeightKg);

            // 8. SEO
            if (dto.Seo is not null)
            {
                product.UpdateSeo(
                    sellerId,
                    dto.Seo.MetaTitle,
                    dto.Seo.MetaDescription,
                    dto.Seo.MetaKeywords,
                    dto.Seo.CanonicalUrl,
                    dto.Seo.OgTitle,
                    dto.Seo.OgDescription,
                    dto.Seo.OgImageUrl);
            }

            // 9. If product was Active → move back to PendingApproval
            //    Admin must review changes before going live again
            var previousStatus = product.Status;

            if (product.Status == ProductStatus.Active)
            {
                product.SendForReapproval(sellerId);

                await _statusHistoryRepository.AddAsync(
                    ProductStatusHistory.Create(
                        productId: id,
                        fromStatus: previousStatus,
                        toStatus: ProductStatus.PendingApproval,
                        changedBy: sellerId,
                        note: "Product edited by seller — sent for re-approval."),
                    ct);
            }

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            var updated = await _productRepository
                .GetByIdWithDetailsAsync(id, ct);

            return Result<ProductDto>.Success(_mapper.Map<ProductDto>(updated!));
        }

        // ══════════════════════════════════════════════════════
        // DELETE — seller can only delete their own
        // ══════════════════════════════════════════════════════

        public async Task<Result> DeleteAsync(
            Guid id, Guid sellerId, CancellationToken ct = default)
        {
            var product = await _productRepository.GetByIdAsync(id, ct);

            if (product is null || product.IsDeleted)
                return Result.Failure(
                    $"Product {id} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            product.SoftDelete(sellerId);
            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ══════════════════════════════════════════════════════
        // ARCHIVE
        // ══════════════════════════════════════════════════════

        public async Task<Result> ArchiveAsync(
            Guid id, Guid sellerId, CancellationToken ct = default)
        {
            var product = await _productRepository.GetByIdAsync(id, ct);

            if (product is null || product.IsDeleted)
                return Result.Failure(
                    $"Product {id} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            var previousStatus = product.Status;

            try { product.Archive(sellerId); }
            catch (DomainException ex)
            { return Result.Failure(ex.Message, ex.Code); }

            await _statusHistoryRepository.AddAsync(
                ProductStatusHistory.Create(
                    productId: id,
                    fromStatus: previousStatus,
                    toStatus: ProductStatus.Archived,
                    changedBy: sellerId,
                    note: "Archived by seller."),
                ct);

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ══════════════════════════════════════════════════════
        // RESUBMIT FOR APPROVAL — after rejection
        // ══════════════════════════════════════════════════════

        public async Task<Result> ResubmitForApprovalAsync(
            Guid id, Guid sellerId, CancellationToken ct = default)
        {
            var product = await _productRepository.GetByIdAsync(id, ct);

            if (product is null || product.IsDeleted)
                return Result.Failure(
                    $"Product {id} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            var previousStatus = product.Status;

            try { product.ResubmitForApproval(sellerId); }
            catch (DomainException ex)
            { return Result.Failure(ex.Message, ex.Code); }

            await _statusHistoryRepository.AddAsync(
                ProductStatusHistory.Create(
                    productId: id,
                    fromStatus: previousStatus,
                    toStatus: ProductStatus.PendingApproval,
                    changedBy: sellerId,
                    note: "Resubmitted for approval by seller."),
                ct);

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ══════════════════════════════════════════════════════
        // VARIANTS — ownership checked on every call
        // ══════════════════════════════════════════════════════

        public async Task<Result<ProductVariantDto>> AddVariantAsync(
            Guid productId,
            CreateProductVariantDto dto,
            Guid sellerId,
            CancellationToken ct = default)
        {
            var product = await _productRepository
                .GetByIdWithDetailsAsync(productId, ct);

            if (product is null || product.IsDeleted)
                return Result<ProductVariantDto>.Failure(
                    $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result<ProductVariantDto>.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            if (!string.IsNullOrWhiteSpace(dto.Sku) &&
                await _productRepository.SkuExistsAsync(dto.Sku, ct))
                return Result<ProductVariantDto>.Failure(
                    $"SKU '{dto.Sku}' already exists.", "SKU_EXISTS");

            if (product.Variants.Any(v =>
                    !v.IsDeleted &&
                    v.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
                return Result<ProductVariantDto>.Failure(
                    $"Variant '{dto.Name}' already exists.", "VARIANT_NAME_EXISTS");

            var price = new Money(dto.Price, dto.CurrencyCode);
            var compareAtPrice = dto.CompareAtPrice.HasValue
                ? new Money(dto.CompareAtPrice.Value, dto.CurrencyCode)
                : null;

            var variant = product.AddVariant(
                name: dto.Name,
                price: price,
                createdBy: sellerId,
                sku: dto.Sku,
                compareAtPrice: compareAtPrice,
                weightKg: dto.WeightKg,
                imageUrl: dto.ImageUrl,
                sortOrder: dto.SortOrder);

            foreach (var attr in dto.Attributes.OrderBy(a => a.SortOrder))
                variant.AddAttribute(attr.Name, attr.Value, attr.SortOrder);

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<ProductVariantDto>.Success(
                _mapper.Map<ProductVariantDto>(variant));
        }

        public async Task<Result> UpdateVariantAsync(
            Guid productId,
            Guid variantId,
            CreateProductVariantDto dto,
            Guid sellerId,
            CancellationToken ct = default)
        {
            var product = await _productRepository
                .GetByIdWithDetailsAsync(productId, ct);

            if (product is null || product.IsDeleted)
                return Result.Failure(
                    $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            var variant = product.Variants
                .FirstOrDefault(v => v.Id == variantId && !v.IsDeleted);

            if (variant is null)
                return Result.Failure(
                    $"Variant {variantId} not found.", "VARIANT_NOT_FOUND");

            if (!string.IsNullOrWhiteSpace(dto.Sku) &&
                dto.Sku != variant.Sku &&
                await _productRepository.SkuExistsAsync(dto.Sku, ct))
                return Result.Failure(
                    $"SKU '{dto.Sku}' already exists.", "SKU_EXISTS");

            var price = new Money(dto.Price, dto.CurrencyCode);
            var compareAtPrice = dto.CompareAtPrice.HasValue
                ? new Money(dto.CompareAtPrice.Value, dto.CurrencyCode)
                : null;

            variant.Update(
                name: dto.Name,
                price: price,
                updatedBy: sellerId,
                sku: dto.Sku,
                compareAtPrice: compareAtPrice,
                weightKg: dto.WeightKg,
                imageUrl: dto.ImageUrl);

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> DeleteVariantAsync(
            Guid productId,
            Guid variantId,
            Guid sellerId,
            CancellationToken ct = default)
        {
            var product = await _productRepository
                .GetByIdWithDetailsAsync(productId, ct);

            if (product is null || product.IsDeleted)
                return Result.Failure(
                    $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            var variant = product.Variants
                .FirstOrDefault(v => v.Id == variantId && !v.IsDeleted);

            if (variant is null)
                return Result.Failure(
                    $"Variant {variantId} not found.", "VARIANT_NOT_FOUND");

            if (product.Variants.Count(v => !v.IsDeleted) == 1)
                return Result.Failure(
                    "Cannot delete the last variant.", "LAST_VARIANT");

            variant.SoftDelete(sellerId);
            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ══════════════════════════════════════════════════════
        // IMAGES
        // ══════════════════════════════════════════════════════

        public async Task<Result<ProductImageDto>> AddImageAsync(
            Guid productId,
            Stream fileStream,
            string fileName,
            string contentType,
            Guid sellerId,
            string? altText = null,
            CancellationToken ct = default)
        {
            if (!IsValidImageContentType(contentType))
                return Result<ProductImageDto>.Failure(
                    "Only JPEG, PNG, GIF and WebP images are allowed.",
                    "INVALID_IMAGE_TYPE");

            var product = await _productRepository
                .GetByIdWithDetailsAsync(productId, ct);

            if (product is null || product.IsDeleted)
                return Result<ProductImageDto>.Failure(
                    $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result<ProductImageDto>.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            if (product.Images.Count >= 10)
                return Result<ProductImageDto>.Failure(
                    "A product cannot have more than 10 images.",
                    "MAX_IMAGES_REACHED");

            var imageUrl = await _storageService.UploadAsync(
                fileStream, fileName, contentType,
                folder: $"products/{productId}", ct);

            var hasPrimary = product.Images.Any(i => i.IsPrimary);

            product.AddImage(
                imageUrl: imageUrl,
                createdBy: sellerId,
                altText: altText,
                sortOrder: product.Images.Count,
                isPrimary: !hasPrimary);

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            var addedImage = product.Images
                .OrderByDescending(i => i.CreatedAt).First();

            return Result<ProductImageDto>.Success(
                _mapper.Map<ProductImageDto>(addedImage));
        }

        public async Task<Result> DeleteImageAsync(
            Guid productId, int imageId,
            Guid sellerId, CancellationToken ct = default)
        {
            var product = await _productRepository
                .GetByIdWithDetailsAsync(productId, ct);

            if (product is null || product.IsDeleted)
                return Result.Failure(
                    $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            var image = product.Images.FirstOrDefault(i => i.Id == imageId);
            if (image is null)
                return Result.Failure(
                    $"Image {imageId} not found.", "IMAGE_NOT_FOUND");

            // Delete from storage
            await _storageService.DeleteAsync(image.ImageUrl, ct);

            if (image.IsPrimary)
            {
                var next = product.Images
                    .Where(i => i.Id != imageId)
                    .OrderBy(i => i.SortOrder)
                    .FirstOrDefault();
                next?.SetAsPrimary();
            }

            product.Images.Remove(image);
            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> ReorderImagesAsync(
            Guid productId,
            List<int> orderedImageIds,
            Guid sellerId,
            CancellationToken ct = default)
        {
            var product = await _productRepository
                .GetByIdWithDetailsAsync(productId, ct);

            if (product is null || product.IsDeleted)
                return Result.Failure(
                    $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            var invalidIds = orderedImageIds
                .Except(product.Images.Select(i => i.Id)).ToList();

            if (invalidIds.Count > 0)
                return Result.Failure(
                    $"Invalid image IDs: {string.Join(", ", invalidIds)}",
                    "INVALID_IMAGE_IDS");

            foreach (var (imageId, index) in
                orderedImageIds.Select((id, i) => (id, i)))
            {
                product.Images.First(i => i.Id == imageId).SetSortOrder(index);
            }

            product.SetUpdatedBy(sellerId);
            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ══════════════════════════════════════════════════════
        // TAGS
        // ══════════════════════════════════════════════════════

        public async Task<Result> SyncTagsAsync(
            Guid productId, List<int> tagIds,
            Guid sellerId, CancellationToken ct = default)
        {
            var product = await _productRepository
                .GetByIdWithDetailsAsync(productId, ct);

            if (product is null || product.IsDeleted)
                return Result.Failure(
                    $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            if (tagIds.Count > 0)
            {
                var tags = await _tagRepository.GetByIdsAsync(tagIds, ct);
                var missingIds = tagIds
                    .Except(tags.Select(t => t.Id)).ToList();

                if (missingIds.Count > 0)
                    return Result.Failure(
                        $"Tags not found: {string.Join(", ", missingIds)}",
                        "TAGS_NOT_FOUND");

                var toRemove = product.Tags
                    .Where(t => !tagIds.Contains(t.Id)).ToList();
                foreach (var tag in toRemove)
                    product.Tags.Remove(tag);

                var existingIds = product.Tags.Select(t => t.Id).ToList();
                var toAdd = tags.Where(t => !existingIds.Contains(t.Id)).ToList();
                foreach (var tag in toAdd)
                    product.Tags.Add(tag);
            }
            else
            {
                product.Tags.Clear();
            }

            product.SetUpdatedBy(sellerId);
            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ══════════════════════════════════════════════════════
        // STATUS HISTORY
        // ══════════════════════════════════════════════════════

        public async Task<Result<IEnumerable<ProductStatusHistoryDto>>>
            GetStatusHistoryAsync(
                Guid id, Guid sellerId, CancellationToken ct = default)
        {
            var product = await _productRepository.GetByIdAsync(id, ct);

            if (product is null || product.IsDeleted)
                return Result<IEnumerable<ProductStatusHistoryDto>>.Failure(
                    $"Product {id} not found.", "PRODUCT_NOT_FOUND");

            if (product.SellerId != sellerId)
                return Result<IEnumerable<ProductStatusHistoryDto>>.Failure(
                    "You do not have access to this product.",
                    "PRODUCT_NOT_FOUND");

            var history = await _statusHistoryRepository
                .GetByProductIdAsync(id, ct);

            return Result<IEnumerable<ProductStatusHistoryDto>>.Success(
                _mapper.Map<IEnumerable<ProductStatusHistoryDto>>(history));
        }

        // ══════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ══════════════════════════════════════════════════════

        private async Task PublishAndClearEventsAsync(
            Product product, CancellationToken ct)
        {
            foreach (var domainEvent in product.DomainEvents)
                await _eventBus.PublishAsync(domainEvent, ct);
            product.ClearDomainEvents();
        }

        private static bool IsValidImageContentType(string contentType)
            => contentType is
                "image/jpeg" or "image/jpg" or
                "image/png" or "image/gif" or "image/webp";
    }

}