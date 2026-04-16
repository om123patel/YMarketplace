// src/Modules/Identity/Identity.Application/Interfaces/ICustomerAddressRepository.cs

using Identity.Domain.Entities;
using Shared.Application.Interfaces;

namespace Identity.Application.Interfaces
{
    public interface ICustomerAddressRepository : IRepository<CustomerAddress, Guid>
    {
        Task<IEnumerable<CustomerAddress>> GetByUserIdAsync(
            Guid userId, CancellationToken ct = default);
        Task<CustomerAddress?> GetDefaultAsync(
            Guid userId, CancellationToken ct = default);
    }
}