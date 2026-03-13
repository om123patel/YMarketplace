using Catalog.Application.DTOs;
using Shared.Application.Models;

namespace Catalog.Application.Services.Interface
{
    public interface IBrandService
    {
        Task<Result<BrandDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<IEnumerable<BrandDto>>> GetAllActiveAsync(CancellationToken ct = default);
        Task<Result<BrandDto>> CreateAsync(CreateBrandDto dto, Guid adminId, CancellationToken ct = default);
        Task<Result<BrandDto>> UpdateAsync(int id, UpdateBrandDto dto, Guid adminId, CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, Guid adminId, CancellationToken ct = default);
        Task<Result> ActivateAsync(int id, Guid adminId, CancellationToken ct = default);
        Task<Result> DeactivateAsync(int id, Guid adminId, CancellationToken ct = default);
    }
}
