// src/Modules/Identity/Identity.Application/Services/Interfaces/ICustomerAuthService.cs
using Identity.Application.DTOs;
using Identity.Application.DTOs.Customer;
using Identity.Application.DTOs.User;
using Shared.Application.Models;

namespace Identity.Application.Services.Interfaces
{
    public interface ICustomerAuthService
    {
        Task<Result<AuthResultDto>> RegisterAsync(
            CustomerRegisterDto dto, CancellationToken ct = default);

        Task<Result<AuthResultDto>> LoginAsync(
            CustomerLoginDto dto, string? ipAddress = null,
            CancellationToken ct = default);

        Task<Result> LogoutAsync(
            string refreshToken, Guid userId,
            CancellationToken ct = default);

        Task<Result<UserDto>> GetMeAsync(
            Guid userId, CancellationToken ct = default);
    }
}