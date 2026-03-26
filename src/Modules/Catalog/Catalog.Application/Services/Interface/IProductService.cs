using Catalog.Application.DTOs.Products;
using Shared.Application.Models;

namespace Catalog.Application.Services.Interface
{
    public interface IProductService
    {
        // ── Product CRUD ──────────────────────────────
        Task<Result<ProductDto>> GetByIdAsync(
            Guid id, CancellationToken ct = default);

        Task<Result<PagedList<ProductListItemDto>>> GetPagedAsync(
            ProductFilterRequest filter, CancellationToken ct = default);

        Task<Result<ProductDto>> CreateByAdminAsync(
            CreateProductDto dto, Guid adminId, CancellationToken ct = default);

        Task<Result<ProductDto>> UpdateAsync(
            Guid id, UpdateProductDto dto, Guid adminId, CancellationToken ct = default);

        Task<Result> DeleteAsync(
            Guid id, Guid adminId, CancellationToken ct = default);

        // ── Approval Workflow ─────────────────────────
        Task<Result> ApproveAsync(
            Guid id, Guid adminId, CancellationToken ct = default);

        Task<Result> RejectAsync(
            Guid id, RejectProductDto dto, Guid adminId, CancellationToken ct = default);

        Task<Result> ArchiveAsync(
            Guid id, Guid adminId, CancellationToken ct = default);

        // ── Feature/Unfeature ─────────────────────────
        Task<Result> FeatureAsync(
            Guid id, Guid adminId, CancellationToken ct = default);

        Task<Result> UnfeatureAsync(
            Guid id, Guid adminId, CancellationToken ct = default);

        // ── Product Variants ──────────────────────────
        Task<Result<ProductVariantDto>> AddVariantAsync(
            Guid productId, CreateProductVariantDto dto,
            Guid adminId, CancellationToken ct = default);

        Task<Result> UpdateVariantAsync(
            Guid productId, Guid variantId,
            CreateProductVariantDto dto,
            Guid adminId, CancellationToken ct = default);

        Task<Result> DeleteVariantAsync(
            Guid productId, Guid variantId,
            Guid adminId, CancellationToken ct = default);

        // ── Product Images ────────────────────────────
        Task<Result<ProductImageDto>> AddImageAsync(
     Guid productId,
     Stream fileStream,
     string fileName,
     string contentType,
     Guid adminId,
     string? altText = null,
     CancellationToken ct = default);

        Task<Result> DeleteImageAsync(
            Guid productId, int imageId,
            Guid adminId, CancellationToken ct = default);

        Task<Result> ReorderImagesAsync(
            Guid productId,
            List<int> orderedImageIds,
            Guid adminId, CancellationToken ct = default);

        Task<Result> SetPrimaryImageAsync(
            Guid productId, int imageId,
            Guid adminId, CancellationToken ct = default);

        // ── Tags ──────────────────────────────────────
        Task<Result> SyncTagsAsync(
            Guid productId, List<int> tagIds,
            Guid adminId, CancellationToken ct = default);

        // ── Status History ────────────────────────────
        Task<Result<IEnumerable<ProductStatusHistoryDto>>> GetStatusHistoryAsync(
            Guid id, CancellationToken ct = default);
    }


}
