using Catalog.Application.DTOs.Brands;
using Catalog.Application.DTOs.Categories;
using Catalog.Application.Services;
using Catalog.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Admin
{
    [Route("api/admin/brands")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminBrandsController : BaseController
    {
        private readonly IBrandService _brandService;

        public AdminBrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet("GetPaged")]
        public async Task<IActionResult> GetPaged(
           [FromQuery] BrandFilterRequest filter,
           CancellationToken ct)
        {
            var result = await _brandService.GetPagedAsync(filter, ct);
            return HandleResult(result);
        }

        // GET api/admin/brands
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _brandService.GetAllActiveAsync(ct);
            return HandleResult(result);
        }

        // GET api/admin/brands/{id}
        [HttpGet("{id:int}", Name = "GetBrandById")]
        public async Task<IActionResult> GetById(
            int id, CancellationToken ct)
        {
            var result = await _brandService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // POST api/admin/brands
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateBrandDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _brandService.CreateAsync(dto, adminId, ct);
            return HandleCreated(result, "GetBrandById",
                new { id = result.Value?.Id });
        }

        // PUT api/admin/brands/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateBrandDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _brandService.UpdateAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        // DELETE api/admin/brands/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _brandService.DeleteAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/brands/{id}/activate
        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(
            int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _brandService.ActivateAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/brands/{id}/deactivate
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(
            int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _brandService.DeactivateAsync(id, adminId, ct);
            return HandleResult(result);
        }
    }
}
