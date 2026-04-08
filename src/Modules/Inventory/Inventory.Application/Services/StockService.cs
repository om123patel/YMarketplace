using AutoMapper;
using FluentValidation;
using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Inventory.Application.Services.Interface;
using Inventory.Application.Validators;
using Inventory.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Inventory.Application.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<AdjustStockDto> _adjustValidator;
        private readonly IValidator<UpdateStockSettingsDto> _settingsValidator;
        private readonly IValidator<CreateStockDto> _createValidator;
        private readonly IValidator<ReserveStockDto> _reserveValidator;

        public StockService(
            IStockRepository stockRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<AdjustStockDto> adjustValidator,
            IValidator<UpdateStockSettingsDto> settingsValidator,
            IValidator<CreateStockDto> createValidator,
            IValidator<ReserveStockDto> reserveValidator)
        {
            _stockRepository = stockRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _adjustValidator = adjustValidator;
            _settingsValidator = settingsValidator;
            _createValidator = createValidator;
            _reserveValidator = reserveValidator;
        }

        public async Task<Result<StockDto>> GetByProductIdAsync(
            Guid productId, CancellationToken ct = default)
        {
            var stock = await _stockRepository.GetByProductIdAsync(productId, ct);
            if (stock is null)
                return Result<StockDto>.Failure(
                    $"Stock record for product {productId} not found.",
                    "STOCK_NOT_FOUND");

            return Result<StockDto>.Success(_mapper.Map<StockDto>(stock));
        }

        public async Task<Result<PagedList<StockListItemDto>>> GetPagedAsync(
            StockFilterRequest filter, CancellationToken ct = default)
        {
            var paged = await _stockRepository.GetPagedAsync(filter, ct);

            var mapped = new PagedList<StockListItemDto>(
                _mapper.Map<List<StockListItemDto>>(paged.Items),
                paged.Page,
                paged.PageSize,
                paged.TotalCount);

            return Result<PagedList<StockListItemDto>>.Success(mapped);
        }

        public async Task<Result<StockDto>> CreateAsync(
            CreateStockDto dto, Guid createdBy,
            CancellationToken ct = default)
        {
            var validation = await _createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<StockDto>.Failure(
                    validation.Errors.First().ErrorMessage,
                    "VALIDATION_FAILED");

            if (await _stockRepository.ExistsForProductAsync(dto.ProductId, ct))
                return Result<StockDto>.Failure(
                    $"Stock record already exists for product {dto.ProductId}.",
                    "STOCK_EXISTS");

            var stock = Stock.Create(
                dto.ProductId,
                createdBy,
                dto.VariantId,
                dto.InitialQuantity,
                dto.LowStockThreshold,
                dto.TrackInventory,
                dto.AllowBackorder);

            await _stockRepository.AddAsync(stock, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<StockDto>.Success(_mapper.Map<StockDto>(stock));
        }

        public async Task<Result<StockDto>> AddStockAsync(
            Guid productId, AdjustStockDto dto,
            Guid updatedBy, CancellationToken ct = default)
        {
            var validation = await _adjustValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<StockDto>.Failure(
                    validation.Errors.First().ErrorMessage,
                    "VALIDATION_FAILED");

            var stock = await _stockRepository.GetByProductIdAsync(productId, ct);
            if (stock is null)
                return Result<StockDto>.Failure(
                    $"Stock record not found for product {productId}.",
                    "STOCK_NOT_FOUND");

            stock.AddStock(dto.Quantity, updatedBy);
            _stockRepository.Update(stock);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<StockDto>.Success(_mapper.Map<StockDto>(stock));
        }

        public async Task<Result<StockDto>> SetQuantityAsync(
            Guid productId, AdjustStockDto dto,
            Guid updatedBy, CancellationToken ct = default)
        {
            // Use the set-quantity validator (allows zero)
            var setValidator = new SetQuantityDtoValidator();
            var validation = await setValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<StockDto>.Failure(
                    validation.Errors.First().ErrorMessage,
                    "VALIDATION_FAILED");

            var stock = await _stockRepository.GetByProductIdAsync(productId, ct);
            if (stock is null)
                return Result<StockDto>.Failure(
                    $"Stock record not found for product {productId}.",
                    "STOCK_NOT_FOUND");

            stock.SetQuantity(dto.Quantity, updatedBy);
            _stockRepository.Update(stock);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<StockDto>.Success(_mapper.Map<StockDto>(stock));
        }

        public async Task<Result<StockDto>> UpdateSettingsAsync(
            Guid productId, UpdateStockSettingsDto dto,
            Guid updatedBy, CancellationToken ct = default)
        {
            var validation = await _settingsValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<StockDto>.Failure(
                    validation.Errors.First().ErrorMessage,
                    "VALIDATION_FAILED");

            var stock = await _stockRepository.GetByProductIdAsync(productId, ct);
            if (stock is null)
                return Result<StockDto>.Failure(
                    $"Stock record not found for product {productId}.",
                    "STOCK_NOT_FOUND");

            stock.UpdateSettings(
                dto.LowStockThreshold,
                dto.TrackInventory,
                dto.AllowBackorder,
                updatedBy);

            _stockRepository.Update(stock);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<StockDto>.Success(_mapper.Map<StockDto>(stock));
        }

        public async Task<Result> ReserveAsync(
            Guid orderId, List<ReserveStockDto> items,
            CancellationToken ct = default)
        {
            // Validate each item
            foreach (var item in items)
            {
                var validation = await _reserveValidator.ValidateAsync(item, ct);
                if (!validation.IsValid)
                    return Result.Failure(
                        validation.Errors.First().ErrorMessage,
                        "VALIDATION_FAILED");
            }

            var stockRecords = new List<Stock>();

            foreach (var item in items)
            {
                var stock = await _stockRepository
                    .GetByProductAndVariantAsync(item.ProductId, item.VariantId, ct);

                if (stock is null)
                    return Result.Failure(
                        $"Stock not found for product {item.ProductId}.",
                        "STOCK_NOT_FOUND");

                if (!stock.AllowBackorder && stock.AvailableQuantity < item.Quantity)
                    return Result.Failure(
                        $"Insufficient stock for product {item.ProductId}. " +
                        $"Available: {stock.AvailableQuantity}, Requested: {item.Quantity}",
                        "INSUFFICIENT_STOCK");

                stockRecords.Add(stock);
            }

            for (int i = 0; i < items.Count; i++)
            {
                stockRecords[i].Reserve(items[i].Quantity);

                var reservation = StockReservation.Create(
                    stockRecords[i].Id,
                    orderId,
                    items[i].Quantity,
                    Guid.Empty);

                stockRecords[i].Reservations.Add(reservation);
                _stockRepository.Update(stockRecords[i]);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> ReleaseReservationAsync(
            Guid orderId, List<ReserveStockDto> items,
            CancellationToken ct = default)
        {
            foreach (var item in items)
            {
                var stock = await _stockRepository
                    .GetByProductAndVariantAsync(item.ProductId, item.VariantId, ct);

                if (stock is null) continue;

                stock.ReleaseReservation(item.Quantity);
                _stockRepository.Update(stock);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> ConfirmDeductionAsync(
            Guid orderId, List<ReserveStockDto> items,
            CancellationToken ct = default)
        {
            foreach (var item in items)
            {
                var stock = await _stockRepository
                    .GetByProductAndVariantAsync(item.ProductId, item.VariantId, ct);

                if (stock is null) continue;

                stock.Deduct(item.Quantity, Guid.Empty);
                _stockRepository.Update(stock);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result<IEnumerable<StockListItemDto>>> GetLowStockAsync(
            CancellationToken ct = default)
        {
            var stocks = await _stockRepository.GetLowStockAsync(ct);
            return Result<IEnumerable<StockListItemDto>>.Success(
                _mapper.Map<IEnumerable<StockListItemDto>>(stocks));
        }
    }
}