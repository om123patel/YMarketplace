using Orders.Application.DTOs.Orders;
using Shared.Application.Models;

namespace Orders.Application.Services.Interface
{
    public interface IOrderService
    {
        // ── Queries ──
        Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Result<PagedList<OrderListItemDto>>> GetPagedAsync(
            OrderFilterRequest filter, CancellationToken ct = default);

        // ── Buyer ──
        Task<Result<OrderDto>> PlaceOrderAsync(
            PlaceOrderDto dto, Guid buyerId, CancellationToken ct = default);
        Task<Result> CancelByBuyerAsync(
            Guid orderId, CancelOrderDto dto, Guid buyerId, CancellationToken ct = default);
        Task<Result<PagedList<OrderListItemDto>>> GetBuyerOrdersAsync(
            Guid buyerId, int page, int pageSize, CancellationToken ct = default);

        // ── Seller ──
        Task<Result> ConfirmAsync(Guid orderId, Guid sellerId, CancellationToken ct = default);
        Task<Result> ShipAsync(
            Guid orderId, ShipOrderDto dto, Guid sellerId, CancellationToken ct = default);
        Task<Result> MarkDeliveredAsync(Guid orderId, Guid sellerId, CancellationToken ct = default);
        Task<Result<PagedList<OrderListItemDto>>> GetStoreOrdersAsync(
            Guid storeId, int page, int pageSize, string? status, CancellationToken ct = default);

        // ── Admin ──
        Task<Result> AdminCancelAsync(
            Guid orderId, CancelOrderDto dto, Guid adminId, CancellationToken ct = default);
        Task<Result> ForceRefundAsync(Guid orderId, Guid adminId, CancellationToken ct = default);
    }

}
