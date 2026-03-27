using AutoMapper;
using FluentValidation;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using Identity.Application.Services.Interfaces;
using Identity.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using Shared.Domain.Primitives;

namespace Identity.Application.Services
{
    public class SellerService : ISellerService
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IIdentityUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateSellerDto> _createValidator;
        private readonly IValidator<UpdateSellerDto> _updateValidator;

        public SellerService(
            ISellerRepository sellerRepository,
            IIdentityUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateSellerDto> createValidator,
            IValidator<UpdateSellerDto> updateValidator)
        {
            _sellerRepository = sellerRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<Result<CreateSellerDto>> CreateAsync(
            CreateSellerDto dto,
            CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<CreateSellerDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Check UserId uniqueness
            if (await _sellerRepository.ExistsByUserIdAsync(dto.UserId, ct))
                return Result<CreateSellerDto>.Failure(
                    "Seller already exists for this user.", "SELLER_ALREADY_EXISTS");

            var address = new Address(
                dto.Address.AddressLine1,
                dto.Address.AddressLine2,
                dto.Address.City,
                dto.Address.State,
                dto.Address.PostalCode,
                dto.Address.Country);

            var seller = Seller.Create(
                dto.UserId,
                dto.BusinessName,
                dto.BusinessEmail,
                dto.BusinessPhone,
                address);

            
            await _sellerRepository.AddAsync(seller, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CreateSellerDto>.Success(_mapper.Map<CreateSellerDto>(seller));
        }

        public async Task<Result<SellerDto>> GetByIdAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var seller = await _sellerRepository.GetByIdAsync(id, ct);

            if (seller is null || seller.IsDeleted)
                return Result<SellerDto>.Failure(
                    $"Seller {id} not found.",
                    "SELLER_NOT_FOUND");

            return Result<SellerDto>.Success(_mapper.Map<SellerDto>(seller));
        }

        public async Task<Result<PagedList<SellerDto>>> GetPagedAsync(
            int page, int pageSize,
            string? status, string? search,
            string? sortBy = null, string? sortDirection = null,
            CancellationToken ct = default)
        {
            var paged = await _sellerRepository.GetPagedAsync(
                page, pageSize, status, search, sortBy, sortDirection, ct);

            var mapped = new PagedList<SellerDto>(
                paged.Items.Select(s => _mapper.Map<SellerDto>(s)).ToList(),
                paged.Page,
                paged.PageSize,
                paged.TotalCount);

            return Result<PagedList<SellerDto>>.Success(mapped);
        }

        public async Task<Result> ApproveAsync(
            Guid id,
            Guid adminId,
            CancellationToken ct = default)
        {
            var seller = await _sellerRepository.GetByIdAsync(id, ct);

            if (seller is null || seller.IsDeleted)
                return Result.Failure("Seller not found.", "SELLER_NOT_FOUND");

            try
            {
                seller.Approve(adminId);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(ex.Message, "INVALID_STATUS_TRANSITION");
            }
            catch (Shared.Domain.Exceptions.DomainException ex)
            {
                return Result.Failure(ex.Message, ex.Code);
            }

            
             _sellerRepository.Update(seller);

            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> RejectAsync(
            Guid id,
            Guid adminId,
            string reason,
            CancellationToken ct = default)
        {
            var seller = await _sellerRepository.GetByIdAsync(id, ct);

            if (seller is null || seller.IsDeleted)
                return Result.Failure("Seller not found.", "SELLER_NOT_FOUND");

            try
            {
                seller.Reject(adminId, reason);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(ex.Message, "INVALID_STATUS_TRANSITION");
            }
            catch (Shared.Domain.Exceptions.DomainException ex)
            {
                return Result.Failure(ex.Message, ex.Code);
            }

            _sellerRepository.Update(seller);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> SuspendAsync(Guid id, CancellationToken ct = default)
        {
            var seller = await _sellerRepository.GetByIdAsync(id, ct);

            if (seller is null || seller.IsDeleted)
                return Result.Failure("Seller not found.", "SELLER_NOT_FOUND");

            try
            {
                seller.Suspend();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(ex.Message, "INVALID_STATUS_TRANSITION");
            }

            _sellerRepository.Update(seller);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> ActivateAsync(Guid id, CancellationToken ct = default)
        {
            var seller = await _sellerRepository.GetByIdAsync(id, ct);

            if (seller is null || seller.IsDeleted)
                return Result.Failure("Seller not found.", "SELLER_NOT_FOUND");

            try
            {
                seller.Activate();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(ex.Message, "INVALID_STATUS_TRANSITION");
            }

            _sellerRepository.Update(seller);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> UpdateAsync(
            UpdateSellerDto command,
            CancellationToken ct = default)
        {
            var seller = await _sellerRepository.GetByIdAsync(command.Id, ct);

            if (seller is null || seller.IsDeleted)
                return Result.Failure("Seller not found.", "SELLER_NOT_FOUND");

            seller.UpdateProfile(
                command.BusinessName,
                command.BusinessEmail,
                command.BusinessPhone,
                command.Description,
                command.WebsiteUrl);

             _sellerRepository.Update(seller);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}