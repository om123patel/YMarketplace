using AdminPanel.Dtos.Orders;
using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("orders")]
    public class OrdersController : Controller
    {
        private readonly IOrderApiClient _orders;
        private readonly AuthTokenService _tokens;

        public OrdersController(IOrderApiClient orders, AuthTokenService tokens)
        {
            _orders = orders;
            _tokens = tokens;
        }

        // GET /orders
        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? search, string? status, string? paymentStatus,
            string sortBy = "createdat", string sortDirection = "desc",
            int page = 1, CancellationToken ct = default)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _orders.GetOrdersAsync(
                token, page, 20, status, paymentStatus, search, sortBy, sortDirection);

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
                Search = search,
                Status = status,
                PaymentStatus = paymentStatus,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            return View(vm);
        }

        // GET /orders/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _orders.GetOrderByIdAsync(token, id);

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

        // POST /orders/{id}/cancel
        [HttpPost("{id:guid}/cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            Guid id, string reason, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _orders.CancelOrderAsync(
                token, id, new CancelOrderRequest { Reason = reason });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Order cancelled." : result?.Error ?? "Failed to cancel.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST /orders/{id}/refund
        [HttpPost("{id:guid}/refund")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceRefund(Guid id, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _orders.ForceRefundAsync(token, id);

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Refund processed." : result?.Error ?? "Failed to refund.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST /orders/disputes/{id}/resolve
        [HttpPost("disputes/{id:guid}/resolve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveDispute(
            Guid id, string resolution, Guid orderId, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _orders.ResolveDisputeAsync(
                token, id, new ResolveDisputeRequest { Resolution = resolution });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Dispute resolved." : result?.Error ?? "Failed to resolve.";

            return RedirectToAction(nameof(Detail), new { id = orderId });
        }

        // POST /orders/disputes/{id}/escalate
        [HttpPost("disputes/{id:guid}/escalate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EscalateDispute(
            Guid id, string note, Guid orderId, CancellationToken ct)
        {
            var token = _tokens.GetAccessToken() ?? "";
            var result = await _orders.EscalateDisputeAsync(
                token, id, new EscalateDisputeRequest { Note = note });

            TempData[result?.Success == true ? "Success" : "Error"] =
                result?.Success == true ? "Dispute escalated." : result?.Error ?? "Failed to escalate.";

            return RedirectToAction(nameof(Detail), new { id = orderId });
        }
    }
}