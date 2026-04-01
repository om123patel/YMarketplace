using AutoMapper;
using Orders.Application.DTOs.Cart;
using Orders.Application.Interfaces;
using Orders.Application.Services.Interface;
using Orders.Domain.Entities;
using Shared.Application.Models;

namespace Orders.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IOrdersUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(
            ICartRepository cartRepo,
            IOrdersUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _cartRepo = cartRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<CartDto>> GetCartAsync(Guid buyerId, CancellationToken ct = default)
        {
            var cart = await _cartRepo.GetByBuyerIdWithItemsAsync(buyerId, ct)
                       ?? Cart.Create(buyerId, buyerId);

            return Result<CartDto>.Success(_mapper.Map<CartDto>(cart));
        }

        public async Task<Result<CartDto>> AddItemAsync(
            Guid buyerId, AddToCartDto dto, CancellationToken ct = default)
        {
            var cart = await _cartRepo.GetByBuyerIdWithItemsAsync(buyerId, ct);

            if (cart is null)
            {
                cart = Cart.Create(buyerId, buyerId);
                await _cartRepo.AddAsync(cart, ct);
            }

            cart.AddItem(dto.ProductId, dto.VariantId, dto.ProductName, dto.VariantName,
                dto.ProductImageUrl, dto.UnitPrice, dto.CurrencyCode, dto.Quantity, buyerId);

            _cartRepo.Update(cart);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<CartDto>.Success(_mapper.Map<CartDto>(cart));
        }

        public async Task<Result<CartDto>> UpdateItemQuantityAsync(
            Guid buyerId, Guid cartItemId, int quantity, CancellationToken ct = default)
        {
            var cart = await _cartRepo.GetByBuyerIdWithItemsAsync(buyerId, ct);
            if (cart is null)
                return Result<CartDto>.Failure("Cart not found.", "CART_NOT_FOUND");

            try { cart.UpdateItemQuantity(cartItemId, quantity, buyerId); }
            catch (Exception ex) { return Result<CartDto>.Failure(ex.Message, "CART_ITEM_NOT_FOUND"); }

            _cartRepo.Update(cart);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<CartDto>.Success(_mapper.Map<CartDto>(cart));
        }

        public async Task<Result> RemoveItemAsync(
            Guid buyerId, Guid cartItemId, CancellationToken ct = default)
        {
            var cart = await _cartRepo.GetByBuyerIdWithItemsAsync(buyerId, ct);
            if (cart is null)
                return Result.Failure("Cart not found.", "CART_NOT_FOUND");

            try { cart.RemoveItem(cartItemId, buyerId); }
            catch (Exception ex) { return Result.Failure(ex.Message, "CART_ITEM_NOT_FOUND"); }

            _cartRepo.Update(cart);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> ClearCartAsync(Guid buyerId, CancellationToken ct = default)
        {
            var cart = await _cartRepo.GetByBuyerIdWithItemsAsync(buyerId, ct);
            if (cart is null)
                return Result.Success(); // already empty

            cart.Clear(buyerId);
            _cartRepo.Update(cart);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
    }

}
