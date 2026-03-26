using Catalog.Application.DTOs.Products;
using Catalog.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Seller
{
    [Route("api/seller/products")]
    [Authorize(Policy = "SellerOnly")]
    public class SellerProductsController : BaseController
    {
        private readonly ISellerProductService _sellerProductService;

        public SellerProductsController(
            ISellerProductService sellerProductService)
        {
            _sellerProductService = sellerProductService;
        }

        // GET api/seller/products
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] SellerProductFilterRequest filter,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .GetPagedAsync(filter, sellerId, ct);
            return HandleResult(result);
        }

        // GET api/seller/products/{id}
        [HttpGet("{id:guid}", Name = "GetSellerProductById")]
        public async Task<IActionResult> GetById(
            Guid id, CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .GetByIdAsync(id, sellerId, ct);
            return HandleResult(result);
        }

        // POST api/seller/products
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateSellerProductDto dto,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();

            // StoreId comes from JWT claim or separate header
            // For now get from JWT claim "storeId"
            var storeIdClaim = User.FindFirst("storeId")?.Value;
            var storeId = Guid.TryParse(storeIdClaim, out var sid)
                ? sid : Guid.Empty;

            var result = await _sellerProductService
                .CreateAsync(dto, sellerId, storeId, ct);

            return HandleCreated(result, "GetSellerProductById",
                new { id = result.Value?.Id });
        }

        // PUT api/seller/products/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateProductDto dto,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .UpdateAsync(id, dto, sellerId, ct);
            return HandleResult(result);
        }

        // DELETE api/seller/products/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            Guid id, CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .DeleteAsync(id, sellerId, ct);
            return HandleResult(result);
        }

        // PATCH api/seller/products/{id}/archive
        [HttpPatch("{id:guid}/archive")]
        public async Task<IActionResult> Archive(
            Guid id, CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .ArchiveAsync(id, sellerId, ct);
            return HandleResult(result);
        }

        // PATCH api/seller/products/{id}/resubmit
        [HttpPatch("{id:guid}/resubmit")]
        public async Task<IActionResult> Resubmit(
            Guid id, CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .ResubmitForApprovalAsync(id, sellerId, ct);
            return HandleResult(result);
        }

        // ── Variants ──

        [HttpPost("{id:guid}/variants")]
        public async Task<IActionResult> AddVariant(
            Guid id,
            [FromBody] CreateProductVariantDto dto,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .AddVariantAsync(id, dto, sellerId, ct);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}/variants/{variantId:guid}")]
        public async Task<IActionResult> UpdateVariant(
            Guid id, Guid variantId,
            [FromBody] CreateProductVariantDto dto,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .UpdateVariantAsync(id, variantId, dto, sellerId, ct);
            return HandleResult(result);
        }

        [HttpDelete("{id:guid}/variants/{variantId:guid}")]
        public async Task<IActionResult> DeleteVariant(
            Guid id, Guid variantId,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .DeleteVariantAsync(id, variantId, sellerId, ct);
            return HandleResult(result);
        }

        // ── Images ──

        [HttpPost("{id:guid}/images")]
        public async Task<IActionResult> AddImage(
            Guid id,
            IFormFile file,
            [FromForm] string? altText,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            using var stream = file.OpenReadStream();

            var result = await _sellerProductService.AddImageAsync(
                productId: id,
                fileStream: stream,
                fileName: file.FileName,
                contentType: file.ContentType,
                sellerId: sellerId,
                altText: altText,
                ct: ct);

            return HandleResult(result);
        }

        [HttpDelete("{id:guid}/images/{imageId:int}")]
        public async Task<IActionResult> DeleteImage(
            Guid id, int imageId, CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .DeleteImageAsync(id, imageId, sellerId, ct);
            return HandleResult(result);
        }

        [HttpPut("{id:guid}/images/reorder")]
        public async Task<IActionResult> ReorderImages(
            Guid id,
            [FromBody] List<int> orderedImageIds,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .ReorderImagesAsync(id, orderedImageIds, sellerId, ct);
            return HandleResult(result);
        }

        // ── Tags ──

        [HttpPut("{id:guid}/tags")]
        public async Task<IActionResult> SyncTags(
            Guid id,
            [FromBody] List<int> tagIds,
            CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .SyncTagsAsync(id, tagIds, sellerId, ct);
            return HandleResult(result);
        }

        // ── Status History ──

        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetStatusHistory(
            Guid id, CancellationToken ct)
        {
            var sellerId = User.GetUserId();
            var result = await _sellerProductService
                .GetStatusHistoryAsync(id, sellerId, ct);
            return HandleResult(result);
        }
    }
}
