using Identity.Application.DTOs;
using Shared.Application.Models;

namespace Identity.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> RegisterAsync(
            RegisterUserDto dto, CancellationToken ct = default);

        Task<Result<AuthResponseDto>> LoginAsync(
            LoginDto dto, CancellationToken ct = default);

        Task<Result<AuthResponseDto>> RefreshTokenAsync(
            RefreshTokenDto dto, CancellationToken ct = default);

        Task<Result> RevokeTokenAsync(
            string refreshToken, CancellationToken ct = default);

        Task<Result<UserDto>> GetCurrentUserAsync(
            Guid userId, CancellationToken ct = default);

        Task<Result<UserDto>> UpdateProfileAsync(
            Guid userId, UpdateProfileDto dto, CancellationToken ct = default);

        Task<Result> ChangePasswordAsync(
            Guid userId, ChangePasswordDto dto, CancellationToken ct = default);
    }

}
