using AdminPanel.Dtos.Inventory;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Common;
using AdminPanel.ViewModels.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("inventory")]
    public class InventoryController : Controller
    {
        
        private readonly IInventoryApiClient _inventory;
        private readonly AuthTokenService _tokens;
        private readonly IValidator<AdjustStockViewModel> _adjustValidator;
        private readonly IValidator<SetQuantityViewModel> _setValidator;
        private readonly IValidator<UpdateStockSettingsViewModel> _settingsValidator;
        private readonly IValidator<CreateStockViewModel> _createValidator;

        public InventoryController(
     IInventoryApiClient inventory,
     AuthTokenService tokens,
     IValidator<AdjustStockViewModel> adjustValidator,
     IValidator<SetQuantityViewModel> setValidator,
     IValidator<UpdateStockSettingsViewModel> settingsValidator,
     IValidator<CreateStockViewModel> createValidator)
        {
            _inventory = inventory;
            _tokens = tokens;
            _adjustValidator = adjustValidator;
            _setValidator = setValidator;
            _settingsValidator = settingsValidator;
            _createValidator = createValidator;
        }

        // GET /inventory
        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? search, string? status,
            bool? lowStockOnly,
            string sortBy = "updatedat",
            string sortDirection = "desc",
            int page = 1,
            CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";

            var resultTask = _inventory.GetPagedAsync(
                token, page, 20, search, status,
                lowStockOnly, sortBy, sortDirection);

            var lowStockTask = _inventory.GetLowStockAsync(token);

            await Task.WhenAll(resultTask, lowStockTask);

            var result = await resultTask;
            var lowStockResult = await lowStockTask;

            var vm = new StockListViewModel
            {
                Items = result?.Data?.Items.Select(s => new StockListItem
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    VariantId = s.VariantId,
                    ProductName = s.ProductName,
                    VariantName = s.VariantName,
                    ProductImageUrl = s.ProductImageUrl,
                    Quantity = s.Quantity,
                    ReservedQuantity = s.ReservedQuantity,
                    AvailableQuantity = s.AvailableQuantity,
                    LowStockThreshold = s.LowStockThreshold,
                    Status = s.Status,
                    IsLowStock = s.IsLowStock,
                    UpdatedAt = s.UpdatedAt
                }).ToList() ?? [],
                Page = result?.Data?.Page ?? page,
                PageSize = result?.Data?.PageSize ?? 20,
                TotalCount = result?.Data?.TotalCount ?? 0,
                Search = search,
                Status = status,
                LowStockOnly = lowStockOnly,
                SortBy = sortBy,
                SortDirection = sortDirection,
                LowStockCount = lowStockResult?.Data?.Count ?? 0
            };

            return View(vm);
        }

        // GET /inventory/product/{productId}
        [HttpGet("product/{productId:guid}")]
        public async Task<IActionResult> Detail(
            Guid productId, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _inventory.GetByProductIdAsync(token, productId);

            if (result?.Data is null)
            {
                TempData["Error"] = "Stock record not found.";
                return RedirectToAction(nameof(Index));
            }

            var s = result.Data;
            var vm = new StockDetailViewModel
            {
                Id = s.Id,
                ProductId = s.ProductId,
                VariantId = s.VariantId,
                Quantity = s.Quantity,
                ReservedQuantity = s.ReservedQuantity,
                AvailableQuantity = s.AvailableQuantity,
                LowStockThreshold = s.LowStockThreshold,
                TrackInventory = s.TrackInventory,
                AllowBackorder = s.AllowBackorder,
                Status = s.Status,
                IsLowStock = s.IsLowStock,
                UpdatedAt = s.UpdatedAt,
                // Pre-fill settings form
                NewLowStockThreshold = s.LowStockThreshold,
                NewTrackInventory = s.TrackInventory,
                NewAllowBackorder = s.AllowBackorder
            };

            return View(vm);
        }

        // POST /inventory/product/{productId}/add
        [HttpPost("product/{productId:guid}/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock(
            Guid productId,
            [Bind("Quantity,Note")] AdjustStockViewModel vm,
            CancellationToken ct)
        {
            vm.ProductId = productId;

            var validation = await _adjustValidator.ValidateAsync(vm, ct);
            if (!validation.IsValid)
            {
                TempData["Error"] = validation.Errors.First().ErrorMessage;
                return RedirectToAction(nameof(Detail), new { productId });
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _inventory.AddStockAsync(
                token, productId,
                new AdjustStockRequest { Quantity = vm.Quantity, Note = vm.Note });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? $"Added {vm.Quantity} units to stock."
                    : result?.Error ?? "Failed to add stock.";

            return RedirectToAction(nameof(Detail), new { productId });
        }

        // POST /inventory/product/{productId}/set
        [HttpPost("product/{productId:guid}/set")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetQuantity(
            Guid productId,
            [Bind("Quantity,Note")] SetQuantityViewModel vm,
            CancellationToken ct)
        {
            vm.ProductId = productId;

            var validation = await _setValidator.ValidateAsync(vm, ct);
            if (!validation.IsValid)
            {
                TempData["Error"] = validation.Errors.First().ErrorMessage;
                return RedirectToAction(nameof(Detail), new { productId });
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _inventory.SetQuantityAsync(
                token, productId,
                new AdjustStockRequest { Quantity = vm.Quantity, Note = vm.Note });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? $"Stock quantity set to {vm.Quantity}."
                    : result?.Error ?? "Failed to set quantity.";

            return RedirectToAction(nameof(Detail), new { productId });
        }

        // POST /inventory/product/{productId}/settings
        [HttpPost("product/{productId:guid}/settings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(
            Guid productId,
            [Bind("LowStockThreshold,TrackInventory,AllowBackorder")]
    UpdateStockSettingsViewModel vm,
            CancellationToken ct)
        {
            vm.ProductId = productId;

            var validation = await _settingsValidator.ValidateAsync(vm, ct);
            if (!validation.IsValid)
            {
                TempData["Error"] = validation.Errors.First().ErrorMessage;
                return RedirectToAction(nameof(Detail), new { productId });
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _inventory.UpdateSettingsAsync(
                token, productId,
                new UpdateStockSettingsRequest
                {
                    LowStockThreshold = vm.LowStockThreshold,
                    TrackInventory = vm.TrackInventory,
                    AllowBackorder = vm.AllowBackorder
                });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Stock settings updated."
                    : result?.Error ?? "Failed to update settings.";

            return RedirectToAction(nameof(Detail), new { productId });
        }

        // POST /inventory/product/{productId}/create
        [HttpPost("product/{productId:guid}/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Guid productId,
            [Bind("InitialQuantity,LowStockThreshold,TrackInventory,AllowBackorder")]
    CreateStockViewModel vm,
            CancellationToken ct)
        {
            vm.ProductId = productId;

            var validation = await _createValidator.ValidateAsync(vm, ct);
            if (!validation.IsValid)
            {
                TempData["Error"] = validation.Errors.First().ErrorMessage;
                return RedirectToAction(nameof(Detail), new { productId });
            }

            var token = _tokens.GetAccessToken() ?? "";
            var result = await _inventory.CreateStockAsync(
                token, productId,
                new CreateStockRequest
                {
                    VariantId = vm.VariantId,
                    InitialQuantity = vm.InitialQuantity
                });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true
                    ? "Stock record created."
                    : result?.Error ?? "Failed to create stock record.";

            return RedirectToAction(nameof(Detail), new { productId });
        }

        
    }
}