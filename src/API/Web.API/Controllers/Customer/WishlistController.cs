// src/API/Web.API/Controllers/Customer/WishlistController.cs

using Catalog.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Customer
{
    [Route("api/buyer/wishlist")]
    [Authorize(Policy = "BuyerOnly")]
    public class WishlistController : BaseController
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        // GET api/buyer/wishlist
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _wishlistService.GetAsync(buyerId, ct);
            return HandleResult(result);
        }

        // POST api/buyer/wishlist/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem(
            [FromBody] AddToWishlistRequest req,
            CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _wishlistService.AddItemAsync(buyerId, req.ProductId, ct);
            return HandleResult(result);
        }

        // DELETE api/buyer/wishlist/items/{productId}
        [HttpDelete("items/{productId:guid}")]
        public async Task<IActionResult> RemoveItem(
            Guid productId, CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _wishlistService.RemoveItemAsync(buyerId, productId, ct);
            return HandleResult(result);
        }

        // DELETE api/buyer/wishlist
        [HttpDelete]
        public async Task<IActionResult> Clear(CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _wishlistService.ClearAsync(buyerId, ct);
            return HandleResult(result);
        }
    }

    public record AddToWishlistRequest(Guid ProductId);
}