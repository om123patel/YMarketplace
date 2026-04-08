using Inventory.Application.DTOs;
using Inventory.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Admin
{
    [Route("api/admin/inventory")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminInventoryController : BaseController
    {
        private readonly IStockService _stockService;

        public AdminInventoryController(IStockService stockService)
            => _stockService = stockService;

        // GET api/admin/inventory
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] StockFilterRequest filter,
            CancellationToken ct)
        {
            var result = await _stockService.GetPagedAsync(filter, ct);
            return HandleResult(result);
        }

        // GET api/admin/inventory/low-stock
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock(CancellationToken ct)
        {
            var result = await _stockService.GetLowStockAsync(ct);
            return HandleResult(result);
        }

        // GET api/admin/inventory/product/{productId}
        [HttpGet("product/{productId:guid}")]
        public async Task<IActionResult> GetByProductId(
            Guid productId, CancellationToken ct)
        {
            var result = await _stockService.GetByProductIdAsync(productId, ct);
            return HandleResult(result);
        }

        [HttpPost("product/{productId:guid}")]
        public async Task<IActionResult> Create(
    Guid productId,
    [FromBody] CreateStockDto dto,
    CancellationToken ct)
        {
            dto.ProductId = productId;          // bind route → dto
            var adminId = User.GetUserId();
            var result = await _stockService.CreateAsync(dto, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/inventory/product/{productId}/add
        [HttpPatch("product/{productId:guid}/add")]
        public async Task<IActionResult> AddStock(
            Guid productId,
            [FromBody] AdjustStockDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _stockService.AddStockAsync(productId, dto, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/inventory/product/{productId}/set
        [HttpPatch("product/{productId:guid}/set")]
        public async Task<IActionResult> SetQuantity(
            Guid productId,
            [FromBody] AdjustStockDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _stockService.SetQuantityAsync(productId, dto, adminId, ct);
            return HandleResult(result);
        }

        // PATCH api/admin/inventory/product/{productId}/settings
        [HttpPatch("product/{productId:guid}/settings")]
        public async Task<IActionResult> UpdateSettings(
            Guid productId,
            [FromBody] UpdateStockSettingsDto dto,
            CancellationToken ct)
        {
            var adminId = User.GetUserId();
            var result = await _stockService.UpdateSettingsAsync(
                productId, dto, adminId, ct);
            return HandleResult(result);
        }
    }

    public class CreateStockRequest
    {
        public Guid? VariantId { get; set; }
        public int InitialQuantity { get; set; }
    }
}