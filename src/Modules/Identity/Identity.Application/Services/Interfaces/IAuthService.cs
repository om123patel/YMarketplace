using Identity.Application.DTOs;
using Shared.Application.Models;

namespace Identity.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResultDto>> RegisterAsync(
            RegisterDto dto, CancellationToken ct = default);
        Task<Result<AuthResultDto>> LoginAsync(
            LoginDto dto, string? ipAddress = null, CancellationToken ct = default);
        Task<Result<AuthResultDto>> RefreshTokenAsync(
            string refreshToken, string? ipAddress = null, CancellationToken ct = default);
        Task<Result> RevokeTokenAsync(
            string refreshToken, Guid userId, CancellationToken ct = default);
    }
}
