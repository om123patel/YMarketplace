// src/Modules/Identity/Identity.Application/Services/Interfaces/ICustomerAddressService.cs

using Identity.Application.DTOs.Address;
using Shared.Application.Models;

namespace Identity.Application.Services.Interfaces
{
    public interface ICustomerAddressService
    {
        Task<Result<IEnumerable<CustomerAddressDto>>> GetAllAsync(
            Guid userId, CancellationToken ct = default);
        Task<Result<CustomerAddressDto>> GetByIdAsync(
            Guid id, Guid userId, CancellationToken ct = default);
        Task<Result<CustomerAddressDto>> CreateAsync(
            CreateAddressDto dto, Guid userId, CancellationToken ct = default);
        Task<Result<CustomerAddressDto>> UpdateAsync(
            Guid id, CreateAddressDto dto, Guid userId, CancellationToken ct = default);
        Task<Result> DeleteAsync(
            Guid id, Guid userId, CancellationToken ct = default);
        Task<Result> SetDefaultAsync(
            Guid id, Guid userId, CancellationToken ct = default);
    }
}