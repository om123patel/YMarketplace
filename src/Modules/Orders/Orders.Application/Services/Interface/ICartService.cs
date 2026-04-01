using Orders.Application.DTOs.Cart;
using Shared.Application.Models;

namespace Orders.Application.Services.Interface
{
    public interface ICartService
    {
        Task<Result<CartDto>> GetCartAsync(Guid buyerId, CancellationToken ct = default);
        Task<Result<CartDto>> AddItemAsync(Guid buyerId, AddToCartDto dto, CancellationToken ct = default);
        Task<Result<CartDto>> UpdateItemQuantityAsync(Guid buyerId, Guid cartItemId, int quantity, CancellationToken ct = default);
        Task<Result> RemoveItemAsync(Guid buyerId, Guid cartItemId, CancellationToken ct = default);
        Task<Result> ClearCartAsync(Guid buyerId, CancellationToken ct = default);
    }
}
