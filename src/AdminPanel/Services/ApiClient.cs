using AdminPanel.Dtos.Attributes;
using AdminPanel.Dtos.Auth;
using AdminPanel.Dtos.Brands;
using AdminPanel.Dtos.Categories;
using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Products;
using AdminPanel.Dtos.Tags;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdminPanel.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ApiClient> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ApiClient(HttpClient http, ILogger<ApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        // ══════════════════════════════════════════════════════════
        // AUTH
        // ══════════════════════════════════════════════════════════

        public async Task<ApiResponse<LoginResult>?> LoginAsync(string email, string password)
            => await PostAsync<ApiResponse<LoginResult>>(
                "api/auth/login", new { email, password }, accessToken: null);

        public async Task<ApiResponse<RefreshResult>?> RefreshTokenAsync(string refreshToken)
            => await PostAsync<ApiResponse<RefreshResult>>(
                "api/auth/refresh", new { refreshToken }, accessToken: null);

        // ══════════════════════════════════════════════════════════
        // PRODUCTS — ADMIN
        // ══════════════════════════════════════════════════════════

        public async Task<ApiResponse<PagedResult<ProductDto>>?> GetProductsAsync(
            string? accessToken, int page, int pageSize,
            string? search, string? status, int? categoryId,
            int? brandId = null, string? creatorType = null,
            string sortBy = "createdat", string sortDirection = "desc")
        {
            var query = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["search"] = search,
                ["status"] = status,
                ["categoryId"] = categoryId?.ToString(),
                ["brandId"] = brandId?.ToString(),
                ["creatorType"] = creatorType,
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return await GetAsync<ApiResponse<PagedResult<ProductDto>>>(
                $"api/admin/products{query}", accessToken);
        }

        public async Task<ApiResponse<ProductDto>?> GetProductByIdAsync(string accessToken, Guid id)
            => await GetAsync<ApiResponse<ProductDto>>($"api/admin/products/{id}", accessToken);

        public async Task<ApiResponse<ProductDto>?> CreateProductAsync(
            string accessToken, CreateProductRequest request)
            => await PostAsync<ApiResponse<ProductDto>>("api/admin/products", request, accessToken);

        public async Task<ApiResponse<ProductDto>?> UpdateProductAsync(
            string accessToken, Guid id, UpdateProductRequest request)
            => await PutAsync<ApiResponse<ProductDto>>($"api/admin/products/{id}", request, accessToken);

        public async Task<ApiResponse?> DeleteProductAsync(string accessToken, Guid id)
            => await DeleteAsync<ApiResponse>($"api/admin/products/{id}", accessToken);

        public async Task<ApiResponse?> ApproveProductAsync(string accessToken, Guid id)
            => await PatchAsync<ApiResponse>($"api/admin/products/{id}/approve", null, accessToken);

        public async Task<ApiResponse?> RejectProductAsync(string accessToken, Guid id, string reason)
            => await PatchAsync<ApiResponse>($"api/admin/products/{id}/reject", new { reason }, accessToken);

        public async Task<ApiResponse?> ArchiveProductAsync(string accessToken, Guid id)
            => await PatchAsync<ApiResponse>($"api/admin/products/{id}/archive", null, accessToken);

        // ══════════════════════════════════════════════════════════
        // PRODUCTS — SELLER
        // ══════════════════════════════════════════════════════════

        public async Task<ApiResponse<PagedResult<ProductDto>>?> GetSellerProductsAsync(
            string accessToken, int page, int pageSize,
            string? search, string? status, int? categoryId,
            string sortBy = "createdat", string sortDirection = "desc")
        {
            var query = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["search"] = search,
                ["status"] = status,
                ["categoryId"] = categoryId?.ToString(),
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return await GetAsync<ApiResponse<PagedResult<ProductDto>>>(
                $"api/seller/products{query}", accessToken);
        }

        public async Task<ApiResponse<ProductDto>?> GetSellerProductByIdAsync(string accessToken, Guid id)
            => await GetAsync<ApiResponse<ProductDto>>($"api/seller/products/{id}", accessToken);

        public async Task<ApiResponse<ProductDto>?> CreateSellerProductAsync(
            string accessToken, CreateProductRequest request)
            => await PostAsync<ApiResponse<ProductDto>>("api/seller/products", request, accessToken);

        public async Task<ApiResponse<ProductDto>?> UpdateSellerProductAsync(
            string accessToken, Guid id, UpdateProductRequest request)
            => await PutAsync<ApiResponse<ProductDto>>($"api/seller/products/{id}", request, accessToken);

        public async Task<ApiResponse?> DeleteSellerProductAsync(string accessToken, Guid id)
            => await DeleteAsync<ApiResponse>($"api/seller/products/{id}", accessToken);

        public async Task<ApiResponse?> ArchiveSellerProductAsync(string accessToken, Guid id)
            => await PatchAsync<ApiResponse>($"api/seller/products/{id}/archive", null, accessToken);

        public async Task<ApiResponse?> ResubmitSellerProductAsync(string accessToken, Guid id)
            => await PatchAsync<ApiResponse>($"api/seller/products/{id}/resubmit", null, accessToken);

        // ══════════════════════════════════════════════════════════
        // CATEGORIES
        // ══════════════════════════════════════════════════════════

        public async Task<ApiResponse<List<CategoryDto>>?> GetCategoriesAsync(string accessToken)
            => await GetAsync<ApiResponse<List<CategoryDto>>>("api/admin/categories", accessToken);

        public async Task<ApiResponse<CategoryDto>?> GetCategoryByIdAsync(string accessToken, int id)
            => await GetAsync<ApiResponse<CategoryDto>>($"api/admin/categories/{id}", accessToken);

        public async Task<ApiResponse<CategoryDto>?> CreateCategoryAsync(
            string accessToken, CreateCategoryRequest request)
            => await PostAsync<ApiResponse<CategoryDto>>("api/admin/categories", request, accessToken);

        public async Task<ApiResponse<CategoryDto>?> UpdateCategoryAsync(
            string accessToken, int id, UpdateCategoryRequest request)
            => await PutAsync<ApiResponse<CategoryDto>>($"api/admin/categories/{id}", request, accessToken);

        public async Task<ApiResponse?> DeleteCategoryAsync(string accessToken, int id)
            => await DeleteAsync<ApiResponse>($"api/admin/categories/{id}", accessToken);

        public async Task<ApiResponse?> ToggleCategoryActiveAsync(string accessToken, int id, bool activate)
            => await PatchAsync<ApiResponse>(
                $"api/admin/categories/{id}/{(activate ? "activate" : "deactivate")}",
                null, accessToken);

        // ══════════════════════════════════════════════════════════
        // BRANDS
        // ══════════════════════════════════════════════════════════

        public async Task<ApiResponse<List<BrandDto>>?> GetBrandsAsync(string accessToken)
            => await GetAsync<ApiResponse<List<BrandDto>>>("api/admin/brands", accessToken);

        public async Task<ApiResponse<BrandDto>?> GetBrandByIdAsync(string accessToken, int id)
            => await GetAsync<ApiResponse<BrandDto>>($"api/admin/brands/{id}", accessToken);

        public async Task<ApiResponse<BrandDto>?> CreateBrandAsync(
            string accessToken, CreateBrandRequest request)
            => await PostAsync<ApiResponse<BrandDto>>("api/admin/brands", request, accessToken);

        public async Task<ApiResponse<BrandDto>?> UpdateBrandAsync(
            string accessToken, int id, UpdateBrandRequest request)
            => await PutAsync<ApiResponse<BrandDto>>($"api/admin/brands/{id}", request, accessToken);

        public async Task<ApiResponse?> DeleteBrandAsync(string accessToken, int id)
            => await DeleteAsync<ApiResponse>($"api/admin/brands/{id}", accessToken);

        public async Task<ApiResponse?> ToggleBrandActiveAsync(string accessToken, int id, bool activate)
            => await PatchAsync<ApiResponse>(
                $"api/admin/brands/{id}/{(activate ? "activate" : "deactivate")}",
                null, accessToken);

        // ══════════════════════════════════════════════════════════
        // TAGS
        // ══════════════════════════════════════════════════════════

        public async Task<ApiResponse<List<TagDto>>?> GetTagsAsync(string accessToken)
            => await GetAsync<ApiResponse<List<TagDto>>>("api/admin/tags", accessToken);

        public async Task<ApiResponse<TagDto>?> GetTagByIdAsync(string accessToken, int id)
            => await GetAsync<ApiResponse<TagDto>>($"api/admin/tags/{id}", accessToken);

        public async Task<ApiResponse<TagDto>?> CreateTagAsync(
            string accessToken, CreateTagRequest request)
            => await PostAsync<ApiResponse<TagDto>>("api/admin/tags", request, accessToken);

        public async Task<ApiResponse<TagDto>?> UpdateTagAsync(
            string accessToken, int id, UpdateTagRequest request)
            => await PutAsync<ApiResponse<TagDto>>($"api/admin/tags/{id}", request, accessToken);

        public async Task<ApiResponse?> DeleteTagAsync(string accessToken, int id)
            => await DeleteAsync<ApiResponse>($"api/admin/tags/{id}", accessToken);

        // ══════════════════════════════════════════════════════════
        // ATTRIBUTE TEMPLATES
        // ══════════════════════════════════════════════════════════

        public async Task<ApiResponse<List<AttributeTemplateDto>>?> GetAttributeTemplatesAsync(
            string accessToken)
            => await GetAsync<ApiResponse<List<AttributeTemplateDto>>>(
                "api/admin/attribute-templates", accessToken);

        public async Task<ApiResponse<AttributeTemplateDto>?> GetAttributeTemplateByIdAsync(
            string accessToken, int id)
            => await GetAsync<ApiResponse<AttributeTemplateDto>>(
                $"api/admin/attribute-templates/{id}", accessToken);

        public async Task<ApiResponse<AttributeTemplateDto>?> CreateAttributeTemplateAsync(
            string accessToken, CreateAttributeTemplateRequest request)
            => await PostAsync<ApiResponse<AttributeTemplateDto>>(
                "api/admin/attribute-templates", request, accessToken);

        public async Task<ApiResponse<AttributeTemplateDto>?> UpdateAttributeTemplateAsync(
            string accessToken, int id, UpdateAttributeTemplateRequest request)
            => await PutAsync<ApiResponse<AttributeTemplateDto>>(
                $"api/admin/attribute-templates/{id}", request, accessToken);

        public async Task<ApiResponse?> DeleteAttributeTemplateAsync(string accessToken, int id)
            => await DeleteAsync<ApiResponse>($"api/admin/attribute-templates/{id}", accessToken);


        // ── ADD THESE METHODS to ApiClient.cs ───────────────────────
        // Place them after the existing Tags region, before private helpers.

        // ══════════════════════════════════════════════════════════
        // USERS
        // ══════════════════════════════════════════════════════════

        public async Task<ApiResponse<PagedResult<UserDto>>?> GetUsersAsync(
            string accessToken, int page, int pageSize,
            string? search = null, string? role = null, string? status = null,
            string sortBy = "createdat", string sortDirection = "desc")
        {
            var query = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["search"] = search,
                ["role"] = role,
                ["status"] = status,
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return await GetAsync<ApiResponse<PagedResult<UserDto>>>(
                $"api/admin/users{query}", accessToken);
        }

        public async Task<ApiResponse<UserDto>?> GetUserByIdAsync(
            string accessToken, Guid id)
            => await GetAsync<ApiResponse<UserDto>>(
                $"api/admin/users/{id}", accessToken);

        public async Task<ApiResponse?> SuspendUserAsync(
            string accessToken, Guid id, string reason)
            => await PatchAsync<ApiResponse>(
                $"api/admin/users/{id}/suspend",
                new { reason }, accessToken);

        public async Task<ApiResponse?> ActivateUserAsync(
            string accessToken, Guid id)
            => await PatchAsync<ApiResponse>(
                $"api/admin/users/{id}/activate", null, accessToken);

        public async Task<ApiResponse?> DeleteUserAsync(
            string accessToken, Guid id)
            => await DeleteAsync<ApiResponse>(
                $"api/admin/users/{id}", accessToken);

        // ══════════════════════════════════════════════════════════
        // SELLERS
        // ══════════════════════════════════════════════════════════

        public async Task<ApiResponse<PagedResult<SellerDto>>?> GetSellersAsync(
            string accessToken, int page, int pageSize,
            string? search = null, string? sellerStatus = null,
            string sortBy = "createdat", string sortDirection = "desc")
        {
            var query = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["search"] = search,
                ["sellerStatus"] = sellerStatus,
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return await GetAsync<ApiResponse<PagedResult<SellerDto>>>(
                $"api/admin/sellers{query}", accessToken);
        }

        public async Task<ApiResponse<SellerDto>?> GetSellerByIdAsync(
            string accessToken, Guid id)
            => await GetAsync<ApiResponse<SellerDto>>(
                $"api/admin/sellers/{id}", accessToken);

        public async Task<ApiResponse?> ApproveSellerAsync(
            string accessToken, Guid id)
            => await PatchAsync<ApiResponse>(
                $"api/admin/sellers/{id}/approve", null, accessToken);

        public async Task<ApiResponse?> RejectSellerAsync(
            string accessToken, Guid id, string reason)
            => await PatchAsync<ApiResponse>(
                $"api/admin/sellers/{id}/reject",
                new { reason }, accessToken);

        public async Task<ApiResponse?> SuspendSellerAsync(
            string accessToken, Guid id, string reason)
            => await PatchAsync<ApiResponse>(
                $"api/admin/sellers/{id}/suspend",
                new { reason }, accessToken);

        public async Task<ApiResponse?> ActivateSellerAsync(
            string accessToken, Guid id)
            => await PatchAsync<ApiResponse>(
                $"api/admin/sellers/{id}/activate", null, accessToken);

        // ══════════════════════════════════════════════════════════
        // PRIVATE HTTP HELPERS
        // ══════════════════════════════════════════════════════════

        private async Task<T?> GetAsync<T>(string url, string? accessToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                AttachToken(request, accessToken);
                var response = await _http.SendAsync(request);
                return await DeserializeAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET {Url} failed", url);
                return default;
            }
        }

        private async Task<T?> PostAsync<T>(string url, object? body, string? accessToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                AttachToken(request, accessToken);
                if (body is not null)
                    request.Content = Serialize(body);
                var response = await _http.SendAsync(request);
                return await DeserializeAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST {Url} failed", url);
                return default;
            }
        }

        private async Task<T?> PutAsync<T>(string url, object? body, string? accessToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, url);
                AttachToken(request, accessToken);
                if (body is not null)
                    request.Content = Serialize(body);
                var response = await _http.SendAsync(request);
                return await DeserializeAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PUT {Url} failed", url);
                return default;
            }
        }

        private async Task<T?> PatchAsync<T>(string url, object? body, string? accessToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Patch, url);
                AttachToken(request, accessToken);
                if (body is not null)
                    request.Content = Serialize(body);
                var response = await _http.SendAsync(request);
                return await DeserializeAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PATCH {Url} failed", url);
                return default;
            }
        }

        private async Task<T?> DeleteAsync<T>(string url, string? accessToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, url);
                AttachToken(request, accessToken);
                var response = await _http.SendAsync(request);
                return await DeserializeAsync<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DELETE {Url} failed", url);
                return default;
            }
        }

        private static void AttachToken(HttpRequestMessage request, string? accessToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
        }

        private static StringContent Serialize(object body)
            => new(JsonSerializer.Serialize(body, JsonOpts), Encoding.UTF8, "application/json");

        private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return default;
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }

        private static string BuildQuery(Dictionary<string, string?> pairs)
        {
            var parts = pairs
                .Where(p => !string.IsNullOrWhiteSpace(p.Value))
                .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}");
            var qs = string.Join("&", parts);
            return qs.Length > 0 ? "?" + qs : "";
        }
    }
}