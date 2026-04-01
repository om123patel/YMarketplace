using AutoMapper;
using FluentValidation;
using Orders.Application.DTOs.Orders;
using Orders.Application.Interfaces;
using Orders.Application.Services.Interface;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Exceptions;

namespace Orders.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IOrdersUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;
        private readonly IValidator<PlaceOrderDto> _placeValidator;
        private readonly IValidator<ShipOrderDto> _shipValidator;

        public OrderService(
            IOrderRepository orderRepo,
            IOrdersUnitOfWork unitOfWork,
            IEventBus eventBus,
            IMapper mapper,
            IValidator<PlaceOrderDto> placeValidator,
            IValidator<ShipOrderDto> shipValidator)
        {
            _orderRepo = orderRepo;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _mapper = mapper;
            _placeValidator = placeValidator;
            _shipValidator = shipValidator;
        }

        public async Task<Result<OrderDto>> GetByIdAsync(
            Guid id, CancellationToken ct = default)
        {
            var order = await _orderRepo.GetByIdWithDetailsAsync(id, ct);
            if (order is null || order.IsDeleted)
                return Result<OrderDto>.Failure($"Order {id} not found.", "ORDER_NOT_FOUND");

            return Result<OrderDto>.Success(_mapper.Map<OrderDto>(order));
        }

        public async Task<Result<PagedList<OrderListItemDto>>> GetPagedAsync(
            OrderFilterRequest filter, CancellationToken ct = default)
        {
            var paged = await _orderRepo.GetPagedAsync(filter, ct);
            var mapped = new PagedList<OrderListItemDto>(
                _mapper.Map<List<OrderListItemDto>>(paged.Items),
                paged.Page, paged.PageSize, paged.TotalCount);
            return Result<PagedList<OrderListItemDto>>.Success(mapped);
        }

        public async Task<Result<PagedList<OrderListItemDto>>> GetBuyerOrdersAsync(
            Guid buyerId, int page, int pageSize, CancellationToken ct = default)
        {
            var paged = await _orderRepo.GetByBuyerIdAsync(buyerId, page, pageSize, ct);
            var mapped = new PagedList<OrderListItemDto>(
                _mapper.Map<List<OrderListItemDto>>(paged.Items),
                paged.Page, paged.PageSize, paged.TotalCount);
            return Result<PagedList<OrderListItemDto>>.Success(mapped);
        }

        public async Task<Result<PagedList<OrderListItemDto>>> GetStoreOrdersAsync(
            Guid storeId, int page, int pageSize, string? status, CancellationToken ct = default)
        {
            var paged = await _orderRepo.GetByStoreIdAsync(storeId, page, pageSize, status, ct);
            var mapped = new PagedList<OrderListItemDto>(
                _mapper.Map<List<OrderListItemDto>>(paged.Items),
                paged.Page, paged.PageSize, paged.TotalCount);
            return Result<PagedList<OrderListItemDto>>.Success(mapped);
        }

        public async Task<Result<OrderDto>> PlaceOrderAsync(
            PlaceOrderDto dto, Guid buyerId, CancellationToken ct = default)
        {
            var validation = await _placeValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<OrderDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            var items = dto.Items.Select(i => (
                i.ProductId, i.VariantId, i.ProductName, i.VariantName,
                i.ProductImageUrl, i.Quantity, i.UnitPrice));

            var order = Order.Place(
                buyerId, dto.StoreId, dto.SellerId, dto.CurrencyCode,
                dto.ShippingAmount, dto.ShippingName, dto.AddressLine1,
                dto.AddressLine2, dto.City, dto.State, dto.PostalCode,
                dto.Country, dto.Phone, items, buyerId);

            await _orderRepo.AddAsync(order, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAndClearEventsAsync(order, ct);

            var created = await _orderRepo.GetByIdWithDetailsAsync(order.Id, ct);
            return Result<OrderDto>.Success(_mapper.Map<OrderDto>(created!));
        }

        public async Task<Result> CancelByBuyerAsync(
            Guid orderId, CancelOrderDto dto, Guid buyerId, CancellationToken ct = default)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order is null || order.IsDeleted)
                return Result.Failure($"Order {orderId} not found.", "ORDER_NOT_FOUND");

            if (order.BuyerId != buyerId)
                return Result.Failure("You do not have access to this order.", "ORDER_NOT_FOUND");

            if (order.Status != OrderStatus.Pending)
                return Result.Failure(
                    "Only pending orders can be cancelled by the buyer.",
                    "INVALID_STATUS_TRANSITION");

            try { order.Cancel(dto.Reason, buyerId); }
            catch (DomainException ex) { return Result.Failure(ex.Message, ex.Code); }

            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAndClearEventsAsync(order, ct);
            return Result.Success();
        }

        public async Task<Result> ConfirmAsync(Guid orderId, Guid sellerId, CancellationToken ct = default)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order is null || order.IsDeleted)
                return Result.Failure($"Order {orderId} not found.", "ORDER_NOT_FOUND");

            if (order.SellerId != sellerId)
                return Result.Failure("You do not have access to this order.", "ORDER_NOT_FOUND");

            try { order.Confirm(sellerId); }
            catch (DomainException ex) { return Result.Failure(ex.Message, ex.Code); }

            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAndClearEventsAsync(order, ct);
            return Result.Success();
        }

        public async Task<Result> ShipAsync(
            Guid orderId, ShipOrderDto dto, Guid sellerId, CancellationToken ct = default)
        {
            var validation = await _shipValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order is null || order.IsDeleted)
                return Result.Failure($"Order {orderId} not found.", "ORDER_NOT_FOUND");

            if (order.SellerId != sellerId)
                return Result.Failure("You do not have access to this order.", "ORDER_NOT_FOUND");

            try { order.MarkAsShipped(dto.TrackingNumber, dto.Carrier, dto.TrackingUrl, sellerId); }
            catch (DomainException ex) { return Result.Failure(ex.Message, ex.Code); }

            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAndClearEventsAsync(order, ct);
            return Result.Success();
        }

        public async Task<Result> MarkDeliveredAsync(Guid orderId, Guid sellerId, CancellationToken ct = default)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order is null || order.IsDeleted)
                return Result.Failure($"Order {orderId} not found.", "ORDER_NOT_FOUND");

            if (order.SellerId != sellerId)
                return Result.Failure("You do not have access to this order.", "ORDER_NOT_FOUND");

            try { order.MarkAsDelivered(sellerId); }
            catch (DomainException ex) { return Result.Failure(ex.Message, ex.Code); }

            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAndClearEventsAsync(order, ct);
            return Result.Success();
        }

        public async Task<Result> AdminCancelAsync(
            Guid orderId, CancelOrderDto dto, Guid adminId, CancellationToken ct = default)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order is null || order.IsDeleted)
                return Result.Failure($"Order {orderId} not found.", "ORDER_NOT_FOUND");

            try { order.Cancel(dto.Reason, adminId); }
            catch (DomainException ex) { return Result.Failure(ex.Message, ex.Code); }

            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);
            await PublishAndClearEventsAsync(order, ct);
            return Result.Success();
        }

        public async Task<Result> ForceRefundAsync(Guid orderId, Guid adminId, CancellationToken ct = default)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order is null || order.IsDeleted)
                return Result.Failure($"Order {orderId} not found.", "ORDER_NOT_FOUND");

            order.MarkPaymentRefunded(adminId);
            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        private async Task PublishAndClearEventsAsync(Order order, CancellationToken ct)
        {
            foreach (var e in order.DomainEvents)
                await _eventBus.PublishAsync(e, ct);
            order.ClearDomainEvents();
        }
    }
}
