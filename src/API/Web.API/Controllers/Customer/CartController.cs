using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.DTOs.Cart;
using Orders.Application.Services.Interface;
using Web.API.Extensions;

namespace Web.API.Controllers.Customer
{
    [Route("api/buyer/cart")]
    [Authorize(Policy = "BuyerOnly")]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET api/buyer/cart
        [HttpGet]
        public async Task<IActionResult> GetCart(CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _cartService.GetCartAsync(buyerId, ct);
            return HandleResult(result);
        }

        // POST api/buyer/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem(
            [FromBody] AddToCartDto dto, CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _cartService.AddItemAsync(buyerId, dto, ct);
            return HandleResult(result);
        }

        // PATCH api/buyer/cart/items/{itemId}
        [HttpPatch("items/{itemId:guid}")]
        public async Task<IActionResult> UpdateQuantity(
            Guid itemId, [FromBody] UpdateCartItemRequest req, CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _cartService.UpdateItemQuantityAsync(buyerId, itemId, req.Quantity, ct);
            return HandleResult(result);
        }

        // DELETE api/buyer/cart/items/{itemId}
        [HttpDelete("items/{itemId:guid}")]
        public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _cartService.RemoveItemAsync(buyerId, itemId, ct);
            return HandleResult(result);
        }

        // DELETE api/buyer/cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart(CancellationToken ct)
        {
            var buyerId = User.GetUserId();
            var result = await _cartService.ClearCartAsync(buyerId, ct);
            return HandleResult(result);
        }
    }

    public record UpdateCartItemRequest(int Quantity);
}