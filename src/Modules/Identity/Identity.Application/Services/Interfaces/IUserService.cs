using Identity.Application.DTOs;
using Shared.Application.Models;

namespace Identity.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<PagedList<UserDto>>> GetPagedAsync(
            int page, int pageSize,
            string? role, string? status, string? search,
            CancellationToken ct = default);
        Task<Result<UserDto>> GetByIdAsync(
            Guid id, CancellationToken ct = default);
        Task<Result> SuspendAsync(
            Guid id, Guid adminId, CancellationToken ct = default);
        Task<Result> ActivateAsync(
            Guid id, Guid adminId, CancellationToken ct = default);
    }
}
