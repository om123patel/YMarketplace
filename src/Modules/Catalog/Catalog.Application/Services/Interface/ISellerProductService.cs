using Catalog.Application.DTOs.Products;
using Shared.Application.Models;

namespace Catalog.Application.Services.Interface
{
    public interface ISellerProductService
    {
        // ── Queries ──
        Task<Result<ProductDto>> GetByIdAsync(
            Guid id, Guid sellerId, CancellationToken ct = default);

        Task<Result<PagedList<ProductListItemDto>>> GetPagedAsync(
            SellerProductFilterRequest filter,
            Guid sellerId,
            CancellationToken ct = default);

        // ── Commands ──
        Task<Result<ProductDto>> CreateAsync(
            CreateSellerProductDto dto,
            Guid sellerId,
            Guid storeId,
            CancellationToken ct = default);

        Task<Result<ProductDto>> UpdateAsync(
            Guid id,
            UpdateProductDto dto,
            Guid sellerId,
            CancellationToken ct = default);

        Task<Result> DeleteAsync(
            Guid id, Guid sellerId, CancellationToken ct = default);

        Task<Result> ArchiveAsync(
            Guid id, Guid sellerId, CancellationToken ct = default);

        Task<Result> ResubmitForApprovalAsync(
            Guid id, Guid sellerId, CancellationToken ct = default);

        // ── Variants ──
        Task<Result<ProductVariantDto>> AddVariantAsync(
            Guid productId,
            CreateProductVariantDto dto,
            Guid sellerId,
            CancellationToken ct = default);

        Task<Result> UpdateVariantAsync(
            Guid productId,
            Guid variantId,
            CreateProductVariantDto dto,
            Guid sellerId,
            CancellationToken ct = default);

        Task<Result> DeleteVariantAsync(
            Guid productId,
            Guid variantId,
            Guid sellerId,
            CancellationToken ct = default);

        // ── Images ──
        Task<Result<ProductImageDto>> AddImageAsync(
            Guid productId,
            Stream fileStream,
            string fileName,
            string contentType,
            Guid sellerId,
            string? altText = null,
            CancellationToken ct = default);

        Task<Result> DeleteImageAsync(
            Guid productId,
            int imageId,
            Guid sellerId,
            CancellationToken ct = default);

        Task<Result> ReorderImagesAsync(
            Guid productId,
            List<int> orderedImageIds,
            Guid sellerId,
            CancellationToken ct = default);

        // ── Tags ──
        Task<Result> SyncTagsAsync(
            Guid productId,
            List<int> tagIds,
            Guid sellerId,
            CancellationToken ct = default);

        // ── Status History ──
        Task<Result<IEnumerable<ProductStatusHistoryDto>>> GetStatusHistoryAsync(
            Guid id, Guid sellerId, CancellationToken ct = default);
    }

}
