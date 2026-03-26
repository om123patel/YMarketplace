using Catalog.Application.DTOs.Products;
using Catalog.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Admin
{
    [Route("api/admin/products")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminProductsController : BaseController
    {
        private readonly IProductService _productService;

        public AdminProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET api/admin/products
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] ProductFilterRequest filter,
            CancellationToken ct)
        {
            var result = await _productService.GetPagedAsync(filter, ct);
            return HandleResult(result);
        }

        // GET api/admin/products/{id}
        [HttpGet("{id:guid}", Name = "GetProductById")]
        public async Task<IActionResult> GetById(
            Guid id, CancellationToken ct)
        {
            var result = await _productService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // POST api/admin/products
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService.CreateByAdminAsync(dto, adminId, ct);
            return HandleCreated(result, "GetProductById",
                new { id = result.Value?.Id });
        }

        // PUT api/admin/products/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateProductDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService.UpdateAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        // DELETE api/admin/products/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            Guid id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService.DeleteAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // ── Approval Workflow ──

        // PATCH api/admin/products/{id}/approve
        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> Approve(
            Guid id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService.ApproveAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/products/{id}/reject
        [HttpPatch("{id:guid}/reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectProductDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService.RejectAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/products/{id}/archive
        [HttpPatch("{id:guid}/archive")]
        public async Task<IActionResult> Archive(
            Guid id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService.ArchiveAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // ── Feature ──

        // PATCH api/admin/products/{id}/feature
        [HttpPatch("{id:guid}/feature")]
        public async Task<IActionResult> Feature(
            Guid id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService.FeatureAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/products/{id}/unfeature
        [HttpPatch("{id:guid}/unfeature")]
        public async Task<IActionResult> Unfeature(
            Guid id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService.UnfeatureAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // ── Variants ──

        // POST api/admin/products/{id}/variants
        [HttpPost("{id:guid}/variants")]
        public async Task<IActionResult> AddVariant(
            Guid id,
            [FromBody] CreateProductVariantDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService.AddVariantAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        // PUT api/admin/products/{id}/variants/{variantId}
        [HttpPut("{id:guid}/variants/{variantId:guid}")]
        public async Task<IActionResult> UpdateVariant(
            Guid id,
            Guid variantId,
            [FromBody] CreateProductVariantDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService
                .UpdateVariantAsync(id, variantId, dto, adminId, ct);
            return HandleResult(result);
        }

        // DELETE api/admin/products/{id}/variants/{variantId}
        [HttpDelete("{id:guid}/variants/{variantId:guid}")]
        public async Task<IActionResult> DeleteVariant(
            Guid id,
            Guid variantId,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService
                .DeleteVariantAsync(id, variantId, adminId, ct);
            return HandleResult(result);
        }

        // ── Images ──

        // POST api/admin/products/{id}/images
        [HttpPost("{id:guid}/images")]
        public async Task<IActionResult> AddImage(
            Guid id,
            IFormFile file,
            [FromForm] string? altText,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();

            using var stream = file.OpenReadStream();

            var result = await _productService.AddImageAsync(
                productId: id,
                fileStream: stream,
                fileName: file.FileName,
                contentType: file.ContentType,
                adminId: adminId,
                altText: altText,
                ct: ct);

            return HandleResult(result);
        }

        // DELETE api/admin/products/{id}/images/{imageId}
        [HttpDelete("{id:guid}/images/{imageId:int}")]
        public async Task<IActionResult> DeleteImage(
            Guid id,
            int imageId,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService
                .DeleteImageAsync(id, imageId, adminId, ct);
            return HandleResult(result);
        }

        // PUT api/admin/products/{id}/images/reorder
        [HttpPut("{id:guid}/images/reorder")]
        public async Task<IActionResult> ReorderImages(
            Guid id,
            [FromBody] List<int> orderedImageIds,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService
                .ReorderImagesAsync(id, orderedImageIds, adminId, ct);
            return HandleResult(result);
        }

        // PUT api/admin/products/{id}/images/{imageId}/primary
        [HttpPut("{id:guid}/images/{imageId:int}/primary")]
        public async Task<IActionResult> SetPrimaryImage(
            Guid id,
            int imageId,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService
                .SetPrimaryImageAsync(id, imageId, adminId, ct);
            return HandleResult(result);
        }

        // ── Tags ──

        // PUT api/admin/products/{id}/tags
        [HttpPut("{id:guid}/tags")]
        public async Task<IActionResult> SyncTags(
            Guid id,
            [FromBody] List<int> tagIds,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _productService
                .SyncTagsAsync(id, tagIds, adminId, ct);
            return HandleResult(result);
        }

        // ── Status History ──

        // GET api/admin/products/{id}/history
        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetStatusHistory(
            Guid id, CancellationToken ct)
        {
            var result = await _productService.GetStatusHistoryAsync(id, ct);
            return HandleResult(result);
        }
    }
}
