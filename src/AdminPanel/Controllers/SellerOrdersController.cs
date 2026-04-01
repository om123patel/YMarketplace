using AdminPanel.Dtos.Orders;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("seller-orders")]
    public class SellerOrdersController : Controller
    {
        private readonly ISellerOrderApiClient _sellerOrders;
        private readonly AuthTokenService _tokens;

        public SellerOrdersController(
            ISellerOrderApiClient sellerOrders, AuthTokenService tokens)
        {
            _sellerOrders = sellerOrders;
            _tokens = tokens;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? status, int page = 1, CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _sellerOrders.GetMyOrdersAsync(token, page, 20, status);

            var vm = new OrderListViewModel
            {
                Items = result?.Data?.Items.Select(o => new OrderListItem
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus,
                    TotalAmount = o.TotalAmount,
                    CurrencyCode = o.CurrencyCode,
                    ItemCount = o.ItemCount,
                    TrackingNumber = o.TrackingNumber,
                    ShippedAt = o.ShippedAt,
                    DeliveredAt = o.DeliveredAt,
                    CreatedAt = o.CreatedAt
                }).ToList() ?? [],
                Page = result?.Data?.Page ?? page,
                PageSize = result?.Data?.PageSize ?? 20,
                TotalCount = result?.Data?.TotalCount ?? 0,
                Status = status
            };

            return View(vm);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _sellerOrders.GetOrderByIdAsync(token, id);
            if (result?.Data is null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var o = result.Data;
            var vm = new OrderDetailViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                BuyerId = o.BuyerId,
                StoreId = o.StoreId,
                SellerId = o.SellerId,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                SubTotal = o.SubTotal,
                ShippingAmount = o.ShippingAmount,
                DiscountAmount = o.DiscountAmount,
                TotalAmount = o.TotalAmount,
                CurrencyCode = o.CurrencyCode,
                ShippingName = o.ShippingName,
                ShippingAddressLine1 = o.ShippingAddressLine1,
                ShippingAddressLine2 = o.ShippingAddressLine2,
                ShippingCity = o.ShippingCity,
                ShippingState = o.ShippingState,
                ShippingPostalCode = o.ShippingPostalCode,
                ShippingCountry = o.ShippingCountry,
                ShippingPhone = o.ShippingPhone,
                TrackingNumber = o.TrackingNumber,
                ShippingCarrier = o.ShippingCarrier,
                TrackingUrl = o.TrackingUrl,
                ShippedAt = o.ShippedAt,
                DeliveredAt = o.DeliveredAt,
                CancelledAt = o.CancelledAt,
                CancellationReason = o.CancellationReason,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                Items = o.Items.Select(i => new OrderDetailItem
                {
                    ProductId = i.ProductId,
                    VariantId = i.VariantId,
                    ProductName = i.ProductName,
                    VariantName = i.VariantName,
                    ProductImageUrl = i.ProductImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal,
                    CurrencyCode = i.CurrencyCode
                }).ToList(),
                Disputes = o.Disputes.Select(d => new OrderDetailDispute
                {
                    Id = d.Id,
                    Reason = d.Reason,
                    BuyerEvidence = d.BuyerEvidence,
                    SellerResponse = d.SellerResponse,
                    AdminNote = d.AdminNote,
                    Status = d.Status,
                    ResolvedAt = d.ResolvedAt,
                    Resolution = d.Resolution,
                    CreatedAt = d.CreatedAt
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost("{id:guid}/confirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _sellerOrders.ConfirmOrderAsync(token, id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Order confirmed." : result?.Error ?? "Failed to confirm.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("{id:guid}/ship")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ship(
            Guid id, string trackingNumber, string carrier,
            string? trackingUrl, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _sellerOrders.ShipOrderAsync(token, id,
                new ShipOrderRequest
                {
                    TrackingNumber = trackingNumber,
                    Carrier = carrier,
                    TrackingUrl = trackingUrl
                });
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Order marked as shipped." : result?.Error ?? "Failed to ship.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("{id:guid}/deliver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDelivered(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _sellerOrders.MarkDeliveredAsync(token, id);
            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Order marked as delivered." : result?.Error ?? "Failed.";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}