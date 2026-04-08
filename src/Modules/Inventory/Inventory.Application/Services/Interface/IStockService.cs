using Inventory.Application.DTOs;
using Shared.Application.Models;

namespace Inventory.Application.Services.Interface
{
    public interface IStockService
    {
        Task<Result<StockDto>> GetByProductIdAsync(
            Guid productId, CancellationToken ct = default);

        Task<Result<PagedList<StockListItemDto>>> GetPagedAsync(
            StockFilterRequest filter, CancellationToken ct = default);

        Task<Result<StockDto>> CreateAsync(
      CreateStockDto dto, Guid createdBy,
      CancellationToken ct = default);

        Task<Result<StockDto>> AddStockAsync(
            Guid productId, AdjustStockDto dto,
            Guid updatedBy, CancellationToken ct = default);

        Task<Result<StockDto>> SetQuantityAsync(
            Guid productId, AdjustStockDto dto,
            Guid updatedBy, CancellationToken ct = default);

        Task<Result<StockDto>> UpdateSettingsAsync(
            Guid productId, UpdateStockSettingsDto dto,
            Guid updatedBy, CancellationToken ct = default);

        Task<Result> ReserveAsync(
            Guid orderId, List<ReserveStockDto> items,
            CancellationToken ct = default);

        Task<Result> ReleaseReservationAsync(
            Guid orderId, List<ReserveStockDto> items,
            CancellationToken ct = default);

        Task<Result> ConfirmDeductionAsync(
            Guid orderId, List<ReserveStockDto> items,
            CancellationToken ct = default);

        Task<Result<IEnumerable<StockListItemDto>>> GetLowStockAsync(
            CancellationToken ct = default);
    }
}