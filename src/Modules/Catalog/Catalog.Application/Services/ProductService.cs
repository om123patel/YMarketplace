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

namespace Catalog.Application.Services;

public class ProductService : IProductService
{
    private readonly IStorageService _storageService;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IProductStatusHistoryRepository _statusHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;
    private readonly IValidator<RejectProductDto> _rejectValidator;
    private readonly IValidator<CreateProductVariantDto> _variantValidator;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        ITagRepository tagRepository,
        IProductStatusHistoryRepository statusHistoryRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IMapper mapper,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator,
        IValidator<RejectProductDto> rejectValidator,
        IValidator<CreateProductVariantDto> variantValidator,
        IStorageService storageService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _tagRepository = tagRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _rejectValidator = rejectValidator;
        _variantValidator = variantValidator;
        _storageService = storageService;
    }

    // ══════════════════════════════════════════════════════
    // QUERIES
    // ══════════════════════════════════════════════════════

    public async Task<Result<ProductDto>> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id, ct);
        if (product is null || product.IsDeleted)
            return Result<ProductDto>.Failure(
                $"Product {id} not found.", "PRODUCT_NOT_FOUND");

        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<Result<PagedList<ProductListItemDto>>> GetPagedAsync(
        ProductFilterRequest filter, CancellationToken ct = default)
    {
        var paged = await _productRepository.GetPagedAsync(filter, ct);

        var mapped = new PagedList<ProductListItemDto>(
            _mapper.Map<List<ProductListItemDto>>(paged.Items),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result<PagedList<ProductListItemDto>>.Success(mapped);
    }

    // ══════════════════════════════════════════════════════
    // CREATE
    // ══════════════════════════════════════════════════════

    public async Task<Result<ProductDto>> CreateByAdminAsync(
        CreateProductDto dto, Guid adminId, CancellationToken ct = default)
    {
        // 1. Validate
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<ProductDto>.Failure(
                string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                "VALIDATION_FAILED");

        // 2. Check category exists
        if (!await _categoryRepository.ExistsAsync(dto.CategoryId, ct))
            return Result<ProductDto>.Failure(
                $"Category {dto.CategoryId} not found.", "CATEGORY_NOT_FOUND");

        // 3. Check brand exists if provided
        if (dto.BrandId.HasValue &&
            !await _brandRepository.ExistsAsync(dto.BrandId.Value, ct))
            return Result<ProductDto>.Failure(
                $"Brand {dto.BrandId} not found.", "BRAND_NOT_FOUND");

        // 4. Check slug uniqueness
        if (await _productRepository.SlugExistsAsync(dto.Slug, ct))
            return Result<ProductDto>.Failure(
                $"Slug '{dto.Slug}' is already taken.", "SLUG_EXISTS");

        // 5. Check SKU uniqueness if provided
        if (!string.IsNullOrWhiteSpace(dto.Sku) &&
            await _productRepository.SkuExistsAsync(dto.Sku, ct))
            return Result<ProductDto>.Failure(
                $"SKU '{dto.Sku}' already exists.", "SKU_EXISTS");

        // 6. Validate all tag IDs exist
        if (dto.TagIds.Count > 0)
        {
            var tags = await _tagRepository.GetByIdsAsync(dto.TagIds, ct);
            var missingIds = dto.TagIds
                .Except(tags.Select(t => t.Id))
                .ToList();

            if (missingIds.Count > 0)
                return Result<ProductDto>.Failure(
                    $"Tags not found: {string.Join(", ", missingIds)}",
                    "TAGS_NOT_FOUND");
        }

        // 7. Build Money value objects
        var basePrice = new Money(dto.BasePrice, dto.CurrencyCode);
        var compareAtPrice = dto.CompareAtPrice.HasValue
            ? new Money(dto.CompareAtPrice.Value, dto.CurrencyCode)
            : null;

        // 8. Create product aggregate via factory
        var product = Product.CreateByAdmin(
            adminId: adminId,
            categoryId: dto.CategoryId,
            name: dto.Name,
            slug: dto.Slug,
            basePrice: basePrice,
            createdBy: adminId,
            brandId: dto.BrandId,
            shortDescription: dto.ShortDescription,
            description: dto.Description,
            sku: dto.Sku,
            isDigital: dto.IsDigital,
            sellerId: dto.SellerId,
            storeId: dto.StoreId);

        // 9. Add variants with their attributes
        foreach (var variantDto in dto.Variants.OrderBy(v => v.SortOrder))
        {
            var variantPrice = new Money(variantDto.Price, variantDto.CurrencyCode);
            var variantCompare = variantDto.CompareAtPrice.HasValue
                ? new Money(variantDto.CompareAtPrice.Value, variantDto.CurrencyCode)
                : null;

            var variant = product.AddVariant(
                name: variantDto.Name,
                price: variantPrice,
                createdBy: adminId,
                sku: variantDto.Sku,
                compareAtPrice: variantCompare,
                weightKg: variantDto.WeightKg,
                imageUrl: variantDto.ImageUrl,
                sortOrder: variantDto.SortOrder);

            foreach (var attr in variantDto.Attributes.OrderBy(a => a.SortOrder))
                variant.AddAttribute(attr.Name, attr.Value, attr.SortOrder);
        }

        // 10. Add images — first one is always primary
        var isPrimary = true;
        foreach (var imageUrl in dto.ImageUrls)
        {
            product.AddImage(imageUrl, adminId, isPrimary: isPrimary);
            isPrimary = false;
        }

        // 11. Update SEO if provided
        if (dto.Seo is not null)
        {
            product.UpdateSeo(
                adminId,
                dto.Seo.MetaTitle,
                dto.Seo.MetaDescription,
                dto.Seo.MetaKeywords,
                dto.Seo.CanonicalUrl,
                dto.Seo.OgTitle,
                dto.Seo.OgDescription,
                dto.Seo.OgImageUrl);
        }

        // 12. Persist product FIRST — the FK on ProductStatusHistory
        //     requires the Products row to exist before history can be inserted.
        await _productRepository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // 13. Log initial status history (product row now committed)
        await _statusHistoryRepository.AddAsync(
            ProductStatusHistory.Create(
                productId: product.Id,
                fromStatus: null,
                toStatus: ProductStatus.Active,
                changedBy: adminId,
                note: "Product created directly by admin."),
            ct);

        await _unitOfWork.SaveChangesAsync(ct);

        // 14. Publish domain events
        await PublishAndClearEventsAsync(product, ct);

        // 15. Reload with full navigation properties for response
        var created = await _productRepository.GetByIdWithDetailsAsync(product.Id, ct);
        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(created!));
    }

    // ══════════════════════════════════════════════════════
    // UPDATE
    // ══════════════════════════════════════════════════════

    public async Task<Result<ProductDto>> UpdateAsync(
        Guid id, UpdateProductDto dto, Guid adminId, CancellationToken ct = default)
    {
        // 1. Validate
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<ProductDto>.Failure(
                string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                "VALIDATION_FAILED");

        // 2. Find product
        var product = await _productRepository.GetByIdWithDetailsAsync(id, ct);
        if (product is null || product.IsDeleted)
            return Result<ProductDto>.Failure(
                $"Product {id} not found.", "PRODUCT_NOT_FOUND");

        // 3. Validate category
        if (!await _categoryRepository.ExistsAsync(dto.CategoryId, ct))
            return Result<ProductDto>.Failure(
                $"Category {dto.CategoryId} not found.", "CATEGORY_NOT_FOUND");

        // 4. Validate brand if provided
        if (dto.BrandId.HasValue &&
            !await _brandRepository.ExistsAsync(dto.BrandId.Value, ct))
            return Result<ProductDto>.Failure(
                $"Brand {dto.BrandId} not found.", "BRAND_NOT_FOUND");

        // 5. Check slug uniqueness — exclude current product
        if (await _productRepository.SlugExistsExceptAsync(dto.Slug, id, ct))
            return Result<ProductDto>.Failure(
                $"Slug '{dto.Slug}' is already taken.", "SLUG_EXISTS");

        // 6. Check SKU uniqueness — exclude current product
        if (!string.IsNullOrWhiteSpace(dto.Sku) &&
            await _productRepository.SkuExistsExceptAsync(dto.Sku, id, ct))
            return Result<ProductDto>.Failure(
                $"SKU '{dto.Sku}' already exists.", "SKU_EXISTS");

        // 7. Build Money value objects
        var basePrice = new Money(dto.BasePrice, dto.CurrencyCode);
        var compareAtPrice = dto.CompareAtPrice.HasValue
            ? new Money(dto.CompareAtPrice.Value, dto.CurrencyCode)
            : null;

        // 8. Update core details via domain method
        product.UpdateDetails(
            name: dto.Name,
            slug: dto.Slug,
            basePrice: basePrice,
            updatedBy: adminId,
            categoryId: dto.CategoryId,
            brandId: dto.BrandId,
            shortDescription: dto.ShortDescription,
            description: dto.Description,
            sku: dto.Sku,
            weightKg: dto.WeightKg);

        // 9. Update SEO
        if (dto.Seo is not null)
        {
            product.UpdateSeo(
                adminId,
                dto.Seo.MetaTitle,
                dto.Seo.MetaDescription,
                dto.Seo.MetaKeywords,
                dto.Seo.CanonicalUrl,
                dto.Seo.OgTitle,
                dto.Seo.OgDescription,
                dto.Seo.OgImageUrl);
        }

        // 10. Sync tags if provided
        if (dto.TagIds.Count > 0)
        {
            var syncResult = await SyncTagsCoreAsync(product, dto.TagIds, adminId, ct);
            if (syncResult.IsFailure)
                return Result<ProductDto>.Failure(syncResult.Error!, syncResult.ErrorCode);
        }

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _productRepository.GetByIdWithDetailsAsync(id, ct);
        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(updated!));
    }

    // ══════════════════════════════════════════════════════
    // DELETE
    // ══════════════════════════════════════════════════════

    public async Task<Result> DeleteAsync(
        Guid id, Guid adminId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure($"Product {id} not found.", "PRODUCT_NOT_FOUND");

        product.SoftDelete(adminId);
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ══════════════════════════════════════════════════════
    // APPROVAL WORKFLOW
    // ══════════════════════════════════════════════════════

    public async Task<Result> ApproveAsync(
        Guid id, Guid adminId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure($"Product {id} not found.", "PRODUCT_NOT_FOUND");

        var previousStatus = product.Status;

        try
        {
            product.Approve(adminId);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message, ex.Code);
        }

        await _statusHistoryRepository.AddAsync(
            ProductStatusHistory.Create(
                productId: id,
                fromStatus: previousStatus,
                toStatus: ProductStatus.Active,
                changedBy: adminId,
                note: "Approved by admin."),
            ct);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);
        await PublishAndClearEventsAsync(product, ct);

        return Result.Success();
    }

    public async Task<Result> RejectAsync(
        Guid id, RejectProductDto dto, Guid adminId, CancellationToken ct = default)
    {
        // 1. Validate rejection reason
        var validation = await _rejectValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Failure(
                string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                "VALIDATION_FAILED");

        // 2. Find product
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure($"Product {id} not found.", "PRODUCT_NOT_FOUND");

        var previousStatus = product.Status;

        try
        {
            product.Reject(adminId, dto.Reason);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message, ex.Code);
        }

        await _statusHistoryRepository.AddAsync(
            ProductStatusHistory.Create(
                productId: id,
                fromStatus: previousStatus,
                toStatus: ProductStatus.Rejected,
                changedBy: adminId,
                note: dto.Reason),
            ct);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);
        await PublishAndClearEventsAsync(product, ct);

        return Result.Success();
    }

    public async Task<Result> ArchiveAsync(
        Guid id, Guid adminId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure($"Product {id} not found.", "PRODUCT_NOT_FOUND");

        var previousStatus = product.Status;

        try
        {
            product.Archive(adminId);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message, ex.Code);
        }

        await _statusHistoryRepository.AddAsync(
            ProductStatusHistory.Create(
                productId: id,
                fromStatus: previousStatus,
                toStatus: ProductStatus.Archived,
                changedBy: adminId,
                note: "Archived by admin."),
            ct);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ══════════════════════════════════════════════════════
    // FEATURE / UNFEATURE
    // ══════════════════════════════════════════════════════

    public async Task<Result> FeatureAsync(
        Guid id, Guid adminId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure($"Product {id} not found.", "PRODUCT_NOT_FOUND");

        if (product.Status != ProductStatus.Active)
            return Result.Failure(
                "Only active products can be featured.",
                "PRODUCT_NOT_ACTIVE");

        product.Feature(adminId);
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> UnfeatureAsync(
        Guid id, Guid adminId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure($"Product {id} not found.", "PRODUCT_NOT_FOUND");

        product.Unfeature(adminId);
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ══════════════════════════════════════════════════════
    // VARIANTS
    // ══════════════════════════════════════════════════════

    public async Task<Result<ProductVariantDto>> AddVariantAsync(
        Guid productId, CreateProductVariantDto dto,
        Guid adminId, CancellationToken ct = default)
    {
        // 1. Validate
        var validation = await _variantValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<ProductVariantDto>.Failure(
                string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                "VALIDATION_FAILED");

        // 2. Find product with details
        var product = await _productRepository.GetByIdWithDetailsAsync(productId, ct);
        if (product is null || product.IsDeleted)
            return Result<ProductVariantDto>.Failure(
                $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

        // 3. Check SKU uniqueness across all products if provided
        if (!string.IsNullOrWhiteSpace(dto.Sku) &&
            await _productRepository.SkuExistsAsync(dto.Sku, ct))
            return Result<ProductVariantDto>.Failure(
                $"SKU '{dto.Sku}' already exists.", "SKU_EXISTS");

        // 4. Check duplicate variant name within same product
        if (product.Variants.Any(v =>
                !v.IsDeleted &&
                v.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
            return Result<ProductVariantDto>.Failure(
                $"A variant named '{dto.Name}' already exists on this product.",
                "VARIANT_NAME_EXISTS");

        // 5. Build Money value objects
        var price = new Money(dto.Price, dto.CurrencyCode);
        var compareAtPrice = dto.CompareAtPrice.HasValue
            ? new Money(dto.CompareAtPrice.Value, dto.CurrencyCode)
            : null;

        // 6. Add variant via aggregate
        var variant = product.AddVariant(
            name: dto.Name,
            price: price,
            createdBy: adminId,
            sku: dto.Sku,
            compareAtPrice: compareAtPrice,
            weightKg: dto.WeightKg,
            imageUrl: dto.ImageUrl,
            sortOrder: dto.SortOrder);

        // 7. Add attributes
        foreach (var attr in dto.Attributes.OrderBy(a => a.SortOrder))
            variant.AddAttribute(attr.Name, attr.Value, attr.SortOrder);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ProductVariantDto>.Success(_mapper.Map<ProductVariantDto>(variant));
    }

    public async Task<Result> UpdateVariantAsync(
        Guid productId, Guid variantId,
        CreateProductVariantDto dto,
        Guid adminId, CancellationToken ct = default)
    {
        // 1. Validate
        var validation = await _variantValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Failure(
                string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                "VALIDATION_FAILED");

        // 2. Find product with variants loaded
        var product = await _productRepository.GetByIdWithDetailsAsync(productId, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure(
                $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

        // 3. Find variant within product
        var variant = product.Variants
            .FirstOrDefault(v => v.Id == variantId && !v.IsDeleted);

        if (variant is null)
            return Result.Failure(
                $"Variant {variantId} not found on product {productId}.",
                "VARIANT_NOT_FOUND");

        // 4. Check SKU uniqueness — exclude this variant's current SKU
        if (!string.IsNullOrWhiteSpace(dto.Sku) &&
            dto.Sku != variant.Sku &&
            await _productRepository.SkuExistsAsync(dto.Sku, ct))
            return Result.Failure(
                $"SKU '{dto.Sku}' already exists.", "SKU_EXISTS");

        // 5. Check duplicate name — exclude current variant
        if (product.Variants.Any(v =>
                v.Id != variantId &&
                !v.IsDeleted &&
                v.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure(
                $"A variant named '{dto.Name}' already exists on this product.",
                "VARIANT_NAME_EXISTS");

        // 6. Build Money value objects
        var price = new Money(dto.Price, dto.CurrencyCode);
        var compareAtPrice = dto.CompareAtPrice.HasValue
            ? new Money(dto.CompareAtPrice.Value, dto.CurrencyCode)
            : null;

        // 7. Update via domain method
        variant.Update(
            name: dto.Name,
            price: price,
            updatedBy: adminId,
            sku: dto.Sku,
            compareAtPrice: compareAtPrice,
            weightKg: dto.WeightKg,
            imageUrl: dto.ImageUrl);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> DeleteVariantAsync(
        Guid productId, Guid variantId,
        Guid adminId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure(
                $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

        var variant = product.Variants
            .FirstOrDefault(v => v.Id == variantId && !v.IsDeleted);

        if (variant is null)
            return Result.Failure(
                $"Variant {variantId} not found on product {productId}.",
                "VARIANT_NOT_FOUND");

        // Prevent deleting the last active variant
        var activeVariants = product.Variants.Count(v => !v.IsDeleted);
        if (activeVariants == 1)
            return Result.Failure(
                "Cannot delete the last variant. A product must have at least one variant.",
                "LAST_VARIANT");

        variant.SoftDelete(adminId);
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ══════════════════════════════════════════════════════
    // IMAGES
    // ══════════════════════════════════════════════════════

    public async Task<Result<ProductImageDto>> AddImageAsync(
     Guid productId, Stream fileStream,        // actual file stream
     string fileName,          // original filename e.g. "phone.jpg"
     string contentType,       // e.g. "image/jpeg"
     Guid adminId,
     string? altText = null,
     CancellationToken ct = default)
    {
        // 1. Validate file
        if (!IsValidImageContentType(contentType))
            return Result<ProductImageDto>.Failure(
                "Only JPEG, PNG, GIF and WebP images are allowed.",
                "INVALID_IMAGE_TYPE");

        if (fileStream.Length > 5 * 1024 * 1024) // 5MB limit
            return Result<ProductImageDto>.Failure(
                "Image size cannot exceed 5MB.",
                "IMAGE_TOO_LARGE");

        // 2. Find product
        var product = await _productRepository
            .GetByIdWithDetailsAsync(productId, ct);

        if (product is null || product.IsDeleted)
            return Result<ProductImageDto>.Failure(
                $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

        if (product.Images.Count >= 10)
            return Result<ProductImageDto>.Failure(
                "A product cannot have more than 10 images.",
                "MAX_IMAGES_REACHED");

        // 3. Upload to storage — gets back a URL
        var imageUrl = await _storageService.UploadAsync(
            fileStream,
            fileName,
            contentType,
            folder: $"products/{productId}",
            ct);

        // 4. Save URL to DB
        var hasPrimary = product.Images.Any(i => i.IsPrimary);
        var sortOrder = product.Images.Count;

        product.AddImage(
            imageUrl: imageUrl,
            createdBy: adminId,
            altText: altText,
            sortOrder: sortOrder,
            isPrimary: !hasPrimary);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        var addedImage = product.Images
            .OrderByDescending(i => i.CreatedAt)
            .First();

        return Result<ProductImageDto>.Success(
            _mapper.Map<ProductImageDto>(addedImage));
    }


    public async Task<Result> DeleteImageAsync(
        Guid productId, int imageId,
        Guid adminId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure(
                $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

        var image = product.Images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            return Result.Failure(
                $"Image {imageId} not found on product {productId}.",
                "IMAGE_NOT_FOUND");

        // If deleting primary image — promote next image to primary
        if (image.IsPrimary)
        {
            var nextImage = product.Images
                .Where(i => i.Id != imageId)
                .OrderBy(i => i.SortOrder)
                .FirstOrDefault();

            nextImage?.SetAsPrimary();
        }

        product.Images.Remove(image);
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> ReorderImagesAsync(
        Guid productId,
        List<int> orderedImageIds,
        Guid adminId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure(
                $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

        // Validate all provided image IDs belong to this product
        var productImageIds = product.Images.Select(i => i.Id).ToList();
        var invalidIds = orderedImageIds.Except(productImageIds).ToList();

        if (invalidIds.Count > 0)
            return Result.Failure(
                $"Image IDs not found on this product: {string.Join(", ", invalidIds)}",
                "INVALID_IMAGE_IDS");

        // Assign new sort orders based on provided order
        foreach (var (imageId, index) in orderedImageIds.Select((id, i) => (id, i)))
        {
            var image = product.Images.First(i => i.Id == imageId);
            image.SetSortOrder(index);  // add SetSortOrder to ProductImage domain entity
        }

        product.SetUpdatedBy(adminId);
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> SetPrimaryImageAsync(
        Guid productId, int imageId,
        Guid adminId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure(
                $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

        var image = product.Images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            return Result.Failure(
                $"Image {imageId} not found on product {productId}.",
                "IMAGE_NOT_FOUND");

        // Unset all existing primary flags
        foreach (var img in product.Images)
            img.UnsetAsPrimary();

        // Set new primary
        image.SetAsPrimary();

        product.SetUpdatedBy(adminId);
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ══════════════════════════════════════════════════════
    // TAGS
    // ══════════════════════════════════════════════════════

    public async Task<Result> SyncTagsAsync(
        Guid productId, List<int> tagIds,
        Guid adminId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId, ct);
        if (product is null || product.IsDeleted)
            return Result.Failure(
                $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

        return await SyncTagsCoreAsync(product, tagIds, adminId, ct);
    }

    // ══════════════════════════════════════════════════════
    // STATUS HISTORY
    // ══════════════════════════════════════════════════════

    public async Task<Result<IEnumerable<ProductStatusHistoryDto>>> GetStatusHistoryAsync(
        Guid id, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product is null || product.IsDeleted)
            return Result<IEnumerable<ProductStatusHistoryDto>>.Failure(
                $"Product {id} not found.", "PRODUCT_NOT_FOUND");

        var history = await _statusHistoryRepository.GetByProductIdAsync(id, ct);

        return Result<IEnumerable<ProductStatusHistoryDto>>.Success(
            _mapper.Map<IEnumerable<ProductStatusHistoryDto>>(history));
    }

    // ══════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ══════════════════════════════════════════════════════

    // Shared tag sync logic used by both CreateByAdmin and UpdateAsync
    private async Task<Result> SyncTagsCoreAsync(
        Product product,
        List<int> tagIds,
        Guid adminId,
        CancellationToken ct)
    {
        // Validate all tag IDs exist
        if (tagIds.Count > 0)
        {
            var tags = await _tagRepository.GetByIdsAsync(tagIds, ct);
            var foundIds = tags.Select(t => t.Id).ToList();
            var missingIds = tagIds.Except(foundIds).ToList();

            if (missingIds.Count > 0)
                return Result.Failure(
                    $"Tags not found: {string.Join(", ", missingIds)}",
                    "TAGS_NOT_FOUND");

            // Remove tags not in new list
            var tagsToRemove = product.Tags
                .Where(t => !tagIds.Contains(t.Id))
                .ToList();

            foreach (var tag in tagsToRemove)
                product.Tags.Remove(tag);

            // Add tags not already on product
            var existingTagIds = product.Tags.Select(t => t.Id).ToList();
            var tagsToAdd = tags
                .Where(t => !existingTagIds.Contains(t.Id))
                .ToList();

            foreach (var tag in tagsToAdd)
                product.Tags.Add(tag);
        }
        else
        {
            // Empty list = remove all tags
            product.Tags.Clear();
        }

        product.SetUpdatedBy(adminId);
        return Result.Success();
    }

    // Publish all pending domain events and clear them from the aggregate
    private async Task PublishAndClearEventsAsync(
        Product product, CancellationToken ct)
    {
        foreach (var domainEvent in product.DomainEvents)
            await _eventBus.PublishAsync(domainEvent, ct);

        product.ClearDomainEvents();
    }

    private static bool IsValidImageContentType(string contentType)
    => contentType is
        "image/jpeg" or
        "image/jpg" or
        "image/png" or
        "image/gif" or
        "image/webp";

}