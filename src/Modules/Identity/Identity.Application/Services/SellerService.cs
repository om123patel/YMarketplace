using AutoMapper;
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
        private readonly ISellerRepository _sellerRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SellerService(
            ISellerRepository sellerRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _sellerRepo = sellerRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Create

        public async Task<Result<Guid>> CreateAsync(
            CreateSellerDto command,
            CancellationToken ct = default)
        {
            var exists = await _sellerRepo.ExistsByUserIdAsync(command.UserId, ct);

            if (exists)
                return Result<Guid>.Failure(
                    "Seller already exists for this user.",
                    "SELLER_ALREADY_EXISTS");

            var address = new Address(
                command.Address.AddressLine1,
                command.Address.AddressLine2,
                command.Address.City,
                command.Address.State,
                command.Address.PostalCode,
                command.Address.Country);

            var seller = Seller.Create(
                command.UserId,
                command.BusinessName,
                command.BusinessEmail,
                command.BusinessPhone,
                address);

            await _sellerRepo.AddAsync(seller, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<Guid>.Success(seller.Id);
        }

        #endregion

        #region Get

        public async Task<Result<SellerDto>> GetByIdAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var seller = await _sellerRepo.GetByIdAsync(id, ct);

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
            var paged = await _sellerRepo.GetPagedAsync(page, pageSize, status, search, sortBy, sortDirection, ct);

            var mapped = new PagedList<SellerDto>(
                paged.Items.Select(s => _mapper.Map<SellerDto>(s)).ToList(),
                paged.Page,
                paged.PageSize,
                paged.TotalCount);

            return Result<PagedList<SellerDto>>.Success(mapped);
        }

        #endregion

        #region Approval

        public async Task<Result> ApproveAsync(
            Guid id,
            Guid adminId,
            CancellationToken ct = default)
        {
            var seller = await _sellerRepo.GetByIdAsync(id, ct);

            if (seller is null || seller.IsDeleted)
                return Result.Failure("Seller not found.", "SELLER_NOT_FOUND");

            try
            {
                seller.Approve(adminId);
            }
            catch (Shared.Domain.Exceptions.DomainException ex)
            {
                return Result.Failure(ex.Message, ex.Code);
            }

            _sellerRepo.Update(seller);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> RejectAsync(
            Guid id,
            Guid adminId,
            string reason,
            CancellationToken ct = default)
        {
            var seller = await _sellerRepo.GetByIdAsync(id, ct);

            if (seller is null || seller.IsDeleted)
                return Result.Failure("Seller not found.", "SELLER_NOT_FOUND");

            try
            {
                seller.Reject(adminId, reason);
            }
            catch (Shared.Domain.Exceptions.DomainException ex)
            {
                return Result.Failure(ex.Message, ex.Code);
            }

            _sellerRepo.Update(seller);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        #endregion

        #region Status

        public async Task<Result> SuspendAsync(Guid id, CancellationToken ct = default)
        {
            var seller = await _sellerRepo.GetByIdAsync(id, ct);

            if (seller is null || seller.IsDeleted)
                return Result.Failure("Seller not found.", "SELLER_NOT_FOUND");

            try
            {
                seller.Suspend();
            }
            catch (Shared.Domain.Exceptions.DomainException ex)
            {
                return Result.Failure(ex.Message, ex.Code);
            }

            _sellerRepo.Update(seller);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> ActivateAsync(Guid id, CancellationToken ct = default)
        {
            var seller = await _sellerRepo.GetByIdAsync(id, ct);

            if (seller is null || seller.IsDeleted)
                return Result.Failure("Seller not found.", "SELLER_NOT_FOUND");

            seller.Activate();

            _sellerRepo.Update(seller);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        #endregion

        #region Update

        public async Task<Result> UpdateAsync(
            UpdateSellerDto command,
            CancellationToken ct = default)
        {
            var seller = await _sellerRepo.GetByIdAsync(command.Id, ct);

            if (seller is null || seller.IsDeleted)
                return Result.Failure("Seller not found.", "SELLER_NOT_FOUND");

            seller.UpdateProfile(
                command.BusinessName,
                command.BusinessEmail,
                command.BusinessPhone,
                command.Description,
                command.WebsiteUrl);

            _sellerRepo.Update(seller);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        #endregion
    }
}
