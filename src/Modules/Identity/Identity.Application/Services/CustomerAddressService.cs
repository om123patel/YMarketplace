// src/Modules/Identity/Identity.Application/Services/CustomerAddressService.cs

using AutoMapper;
using Identity.Application.DTOs.Address;
using Identity.Application.Interfaces;
using Identity.Application.Services.Interfaces;
using Identity.Domain.Entities;
using Shared.Application.Models;

namespace Identity.Application.Services
{
    public class CustomerAddressService : ICustomerAddressService
    {
        private readonly ICustomerAddressRepository _addressRepo;
        private readonly IIdentityUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerAddressService(
            ICustomerAddressRepository addressRepo,
            IIdentityUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _addressRepo = addressRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<CustomerAddressDto>>> GetAllAsync(
            Guid userId, CancellationToken ct = default)
        {
            var addresses = await _addressRepo.GetByUserIdAsync(userId, ct);
            return Result<IEnumerable<CustomerAddressDto>>.Success(
                _mapper.Map<IEnumerable<CustomerAddressDto>>(addresses));
        }

        public async Task<Result<CustomerAddressDto>> GetByIdAsync(
            Guid id, Guid userId, CancellationToken ct = default)
        {
            var address = await _addressRepo.GetByIdAsync(id, ct);
            if (address is null || address.IsDeleted || address.UserId != userId)
                return Result<CustomerAddressDto>.Failure(
                    "Address not found.", "NOT_FOUND");

            return Result<CustomerAddressDto>.Success(
                _mapper.Map<CustomerAddressDto>(address));
        }

        public async Task<Result<CustomerAddressDto>> CreateAsync(
            CreateAddressDto dto, Guid userId, CancellationToken ct = default)
        {
            // Max 10 addresses per user
            var existing = (await _addressRepo.GetByUserIdAsync(userId, ct)).ToList();
            if (existing.Count >= 10)
                return Result<CustomerAddressDto>.Failure(
                    "Maximum of 10 addresses allowed.", "VALIDATION_FAILED");

            // If first address or marked as default → unset others
            if (dto.IsDefault || !existing.Any())
            {
                foreach (var addr in existing.Where(a => a.IsDefault))
                {
                    addr.UnsetAsDefault();
                    _addressRepo.Update(addr);
                }
            }

            var address = CustomerAddress.Create(
                userId, dto.FullName, dto.Phone,
                dto.AddressLine1, dto.AddressLine2,
                dto.City, dto.State, dto.PostalCode, dto.Country,
                dto.IsDefault || !existing.Any(),
                dto.Label, userId);

            await _addressRepo.AddAsync(address, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CustomerAddressDto>.Success(
                _mapper.Map<CustomerAddressDto>(address));
        }

        public async Task<Result<CustomerAddressDto>> UpdateAsync(
            Guid id, CreateAddressDto dto, Guid userId, CancellationToken ct = default)
        {
            var address = await _addressRepo.GetByIdAsync(id, ct);
            if (address is null || address.IsDeleted || address.UserId != userId)
                return Result<CustomerAddressDto>.Failure(
                    "Address not found.", "NOT_FOUND");

            if (dto.IsDefault && !address.IsDefault)
            {
                var others = await _addressRepo.GetByUserIdAsync(userId, ct);
                foreach (var a in others.Where(a => a.Id != id && a.IsDefault))
                {
                    a.UnsetAsDefault();
                    _addressRepo.Update(a);
                }
                address.SetAsDefault();
            }

            address.Update(
                dto.FullName, dto.Phone,
                dto.AddressLine1, dto.AddressLine2,
                dto.City, dto.State, dto.PostalCode,
                dto.Country, dto.Label, userId);

            _addressRepo.Update(address);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CustomerAddressDto>.Success(
                _mapper.Map<CustomerAddressDto>(address));
        }

        public async Task<Result> DeleteAsync(
            Guid id, Guid userId, CancellationToken ct = default)
        {
            var address = await _addressRepo.GetByIdAsync(id, ct);
            if (address is null || address.IsDeleted || address.UserId != userId)
                return Result.Failure("Address not found.", "NOT_FOUND");

            address.SoftDelete(userId);
            _addressRepo.Update(address);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> SetDefaultAsync(
            Guid id, Guid userId, CancellationToken ct = default)
        {
            var addresses = (await _addressRepo.GetByUserIdAsync(userId, ct)).ToList();
            var target = addresses.FirstOrDefault(a => a.Id == id);

            if (target is null || target.IsDeleted)
                return Result.Failure("Address not found.", "NOT_FOUND");

            foreach (var a in addresses.Where(a => a.IsDefault))
            {
                a.UnsetAsDefault();
                _addressRepo.Update(a);
            }

            target.SetAsDefault();
            _addressRepo.Update(target);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}