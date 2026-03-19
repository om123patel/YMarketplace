using AdminPanel.Dtos.Attributes;
using AdminPanel.Dtos.Auth;
using AdminPanel.Dtos.Brands;
using AdminPanel.Dtos.Categories;
using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Products;
using AdminPanel.Dtos.Tags;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface IApiClient
    {
        Task<ApiResponse<LoginResult>?> LoginAsync(string email, string password);
        Task<ApiResponse<RefreshResult>?> RefreshTokenAsync(string refreshToken);
    }
}