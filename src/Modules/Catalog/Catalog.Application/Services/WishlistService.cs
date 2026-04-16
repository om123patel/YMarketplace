// src/Modules/Catalog/Catalog.Application/Services/WishlistService.cs

using Catalog.Application.DTOs.Wishlist;
using Catalog.Application.Interfaces;
using Catalog.Application.Services.Interface;
using Catalog.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Catalog.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepo;
        private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _unitOfWork;

        public WishlistService(
            IWishlistRepository wishlistRepo,
            IProductRepository productRepo,
            IUnitOfWork unitOfWork)
        {
            _wishlistRepo = wishlistRepo;
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<WishlistDto>> GetAsync(
            Guid buyerId, CancellationToken ct = default)
        {
            var wishlist = await _wishlistRepo
                .GetByBuyerIdWithItemsAsync(buyerId, ct)
                ?? Wishlist.Create(buyerId, buyerId);

            return Result<WishlistDto>.Success(await MapAsync(wishlist, ct));
        }

        public async Task<Result<WishlistDto>> AddItemAsync(
            Guid buyerId, Guid productId, CancellationToken ct = default)
        {
            // Validate product exists and is active
            var product = await _productRepo.GetByIdAsync(productId, ct);
            if (product is null || product.IsDeleted)
                return Result<WishlistDto>.Failure(
                    $"Product {productId} not found.", "PRODUCT_NOT_FOUND");

            var wishlist = await _wishlistRepo.GetByBuyerIdWithItemsAsync(buyerId, ct);

            if (wishlist is null)
            {
                wishlist = Wishlist.Create(buyerId, buyerId);
                await _wishlistRepo.AddAsync(wishlist, ct);
            }

            wishlist.AddItem(productId, buyerId);
            _wishlistRepo.Update(wishlist);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<WishlistDto>.Success(await MapAsync(wishlist, ct));
        }

        public async Task<Result> RemoveItemAsync(
            Guid buyerId, Guid productId, CancellationToken ct = default)
        {
            var wishlist = await _wishlistRepo.GetByBuyerIdWithItemsAsync(buyerId, ct);
            if (wishlist is null)
                return Result.Success(); // nothing to remove

            wishlist.RemoveItem(productId, buyerId);
            _wishlistRepo.Update(wishlist);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> ClearAsync(
            Guid buyerId, CancellationToken ct = default)
        {
            var wishlist = await _wishlistRepo.GetByBuyerIdWithItemsAsync(buyerId, ct);
            if (wishlist is null)
                return Result.Success();

            wishlist.Items.Clear();
            wishlist.SetUpdatedBy(buyerId);
            _wishlistRepo.Update(wishlist);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ── Private ──────────────────────────────────────────────
        private async Task<WishlistDto> MapAsync(
            Wishlist wishlist, CancellationToken ct)
        {
            var dto = new WishlistDto
            {
                Id = wishlist.Id,
                BuyerId = wishlist.BuyerId,
                Items = []
            };

            foreach (var item in wishlist.Items)
            {
                var product = await _productRepo.GetByIdWithDetailsAsync(item.ProductId, ct);

                dto.Items.Add(new WishlistItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = product?.Name,
                    ProductSlug = product?.Slug,
                    BasePrice = product?.BasePrice.Amount,
                    CurrencyCode = product?.BasePrice.CurrencyCode,
                    PrimaryImageUrl = product?.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                        ?? product?.Images.FirstOrDefault()?.ImageUrl,
                    IsAvailable = product is not null && product.IsActive,
                    AddedAt = item.CreatedAt
                });
            }

            return dto;
        }
    }
}