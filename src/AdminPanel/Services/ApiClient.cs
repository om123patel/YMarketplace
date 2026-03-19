// AdminPanel/Services/ApiClient.cs
using AdminPanel.Dtos.Auth;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class ApiClient : ApiClientBase, IApiClient
    {
        public ApiClient(HttpClient http, ILogger<ApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<LoginResult>?> LoginAsync(
            string email, string password)
            => PostAsync<ApiResponse<LoginResult>>(
                "api/auth/login", new { email, password }, null);

        public Task<ApiResponse<RefreshResult>?> RefreshTokenAsync(
            string refreshToken)
            => PostAsync<ApiResponse<RefreshResult>>(
                "api/auth/refresh", new { refreshToken }, null);
    }
}