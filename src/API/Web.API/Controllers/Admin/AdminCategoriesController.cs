using Catalog.Application.DTOs;
using Catalog.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Admin
{
    [Route("api/admin/categories")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminCategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET api/admin/categories
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _categoryService.GetAllActiveAsync(ct);
            return HandleResult(result);
        }

        // GET api/admin/categories/roots
        [HttpGet("roots")]
        public async Task<IActionResult> GetRoots(CancellationToken ct)
        {
            var result = await _categoryService.GetRootCategoriesAsync(ct);
            return HandleResult(result);
        }

        // GET api/admin/categories/{id}
        [HttpGet("{id:int}", Name = "GetCategoryById")]
        public async Task<IActionResult> GetById(
            int id, CancellationToken ct)
        {
            var result = await _categoryService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // GET api/admin/categories/{id}/children
        [HttpGet("{id:int}/children")]
        public async Task<IActionResult> GetChildren(
            int id, CancellationToken ct)
        {
            var result = await _categoryService.GetChildrenAsync(id, ct);
            return HandleResult(result);
        }

        // POST api/admin/categories
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateCategoryDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _categoryService.CreateAsync(dto, adminId, ct);
            return HandleCreated(result, "GetCategoryById",
                new { id = result.Value?.Id });
        }

        // PUT api/admin/categories/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCategoryDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _categoryService.UpdateAsync(id, dto, adminId, ct);
            return HandleResult(result);
        }

        // DELETE api/admin/categories/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _categoryService.DeleteAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/categories/{id}/activate
        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(
            int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _categoryService.ActivateAsync(id, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/categories/{id}/deactivate
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(
            int id, CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _categoryService.DeactivateAsync(id, adminId, ct);
            return HandleResult(result);
        }
    }
}
