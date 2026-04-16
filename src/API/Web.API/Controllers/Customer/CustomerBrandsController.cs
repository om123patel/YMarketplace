// src/API/Web.API/Controllers/Customer/CustomerBrandsController.cs

using Catalog.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers.Customer
{
    [Route("api/brands")]
    public class CustomerBrandsController : BaseController
    {
        private readonly IBrandService _brandService;

        public CustomerBrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        // GET api/brands  — all active brands
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _brandService.GetAllActiveAsync(ct);
            return HandleResult(result);
        }

        // GET api/brands/{id}
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _brandService.GetByIdAsync(id, ct);
            return HandleResult(result);
        }
    }
}