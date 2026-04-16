// src/API/Web.API/Controllers/Customer/CustomerCategoriesController.cs

using Catalog.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers.Customer
{
    [Route("api/categories")]
    public class CustomerCategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CustomerCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET api/categories  — all active root categories with children
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _categoryService.GetRootCategoriesAsync(ct);
            return HandleResult(result);
        }

        // GET api/categories/roots  ← fixes the frontend call
        [HttpGet("roots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoots(CancellationToken ct)
        {
            var result = await _categoryService.GetRootCategoriesAsync(ct);
            return HandleResult(result);
        }

        // GET api/categories/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _categoryService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }

        // GET api/categories/{id}/children
        [HttpGet("{id:int}/children")]
        [AllowAnonymous]
        public async Task<IActionResult> GetChildren(int id, CancellationToken ct)
        {
            var result = await _categoryService.GetChildrenAsync(id, ct);
            return HandleResult(result);
        }
    }
}