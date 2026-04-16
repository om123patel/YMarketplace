// src/Modules/Catalog/Catalog.Application/Services/Interface/IWishlistService.cs

using Catalog.Application.DTOs.Wishlist;
using Shared.Application.Models;

namespace Catalog.Application.Services.Interface
{
    public interface IWishlistService
    {
        Task<Result<WishlistDto>> GetAsync(
            Guid buyerId, CancellationToken ct = default);
        Task<Result<WishlistDto>> AddItemAsync(
            Guid buyerId, Guid productId, CancellationToken ct = default);
        Task<Result> RemoveItemAsync(
            Guid buyerId, Guid productId, CancellationToken ct = default);
        Task<Result> ClearAsync(
            Guid buyerId, CancellationToken ct = default);
    }
}