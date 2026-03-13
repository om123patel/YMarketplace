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
        // ── Auth ──────────────────────────────────────────────────
        Task<ApiResponse<LoginResult>?> LoginAsync(string email, string password);
        Task<ApiResponse<RefreshResult>?> RefreshTokenAsync(string refreshToken);

        // ── Products (Admin) ──────────────────────────────────────
        Task<ApiResponse<PagedResult<ProductDto>>?> GetProductsAsync(
            string? accessToken, int page, int pageSize,
            string? search, string? status, int? categoryId,
            int? brandId = null, string? creatorType = null,
            string sortBy = "createdat", string sortDirection = "desc");
        Task<ApiResponse<ProductDto>?> GetProductByIdAsync(string accessToken, Guid id);
        Task<ApiResponse<ProductDto>?> CreateProductAsync(string accessToken, CreateProductRequest request);
        Task<ApiResponse<ProductDto>?> UpdateProductAsync(string accessToken, Guid id, UpdateProductRequest request);
        Task<ApiResponse?> DeleteProductAsync(string accessToken, Guid id);
        Task<ApiResponse?> ApproveProductAsync(string accessToken, Guid id);
        Task<ApiResponse?> RejectProductAsync(string accessToken, Guid id, string reason);
        Task<ApiResponse?> ArchiveProductAsync(string accessToken, Guid id);

        // ── Products (Seller) ─────────────────────────────────────
        Task<ApiResponse<PagedResult<ProductDto>>?> GetSellerProductsAsync(
            string accessToken, int page, int pageSize,
            string? search, string? status, int? categoryId,
            string sortBy = "createdat", string sortDirection = "desc");
        Task<ApiResponse<ProductDto>?> GetSellerProductByIdAsync(string accessToken, Guid id);
        Task<ApiResponse<ProductDto>?> CreateSellerProductAsync(string accessToken, CreateProductRequest request);
        Task<ApiResponse<ProductDto>?> UpdateSellerProductAsync(string accessToken, Guid id, UpdateProductRequest request);
        Task<ApiResponse?> DeleteSellerProductAsync(string accessToken, Guid id);
        Task<ApiResponse?> ArchiveSellerProductAsync(string accessToken, Guid id);
        Task<ApiResponse?> ResubmitSellerProductAsync(string accessToken, Guid id);

        // ── Categories ────────────────────────────────────────────
        Task<ApiResponse<List<CategoryDto>>?> GetCategoriesAsync(string accessToken);
        Task<ApiResponse<CategoryDto>?> GetCategoryByIdAsync(string accessToken, int id);
        Task<ApiResponse<CategoryDto>?> CreateCategoryAsync(string accessToken, CreateCategoryRequest request);
        Task<ApiResponse<CategoryDto>?> UpdateCategoryAsync(string accessToken, int id, UpdateCategoryRequest request);
        Task<ApiResponse?> DeleteCategoryAsync(string accessToken, int id);
        Task<ApiResponse?> ToggleCategoryActiveAsync(string accessToken, int id, bool activate);

        // ── Brands ────────────────────────────────────────────────
        Task<ApiResponse<List<BrandDto>>?> GetBrandsAsync(string accessToken);
        Task<ApiResponse<BrandDto>?> GetBrandByIdAsync(string accessToken, int id);
        Task<ApiResponse<BrandDto>?> CreateBrandAsync(string accessToken, CreateBrandRequest request);
        Task<ApiResponse<BrandDto>?> UpdateBrandAsync(string accessToken, int id, UpdateBrandRequest request);
        Task<ApiResponse?> DeleteBrandAsync(string accessToken, int id);
        Task<ApiResponse?> ToggleBrandActiveAsync(string accessToken, int id, bool activate);

        // ── Tags ──────────────────────────────────────────────────
        Task<ApiResponse<List<TagDto>>?> GetTagsAsync(string accessToken);
        Task<ApiResponse<TagDto>?> GetTagByIdAsync(string accessToken, int id);
        Task<ApiResponse<TagDto>?> CreateTagAsync(string accessToken, CreateTagRequest request);
        Task<ApiResponse<TagDto>?> UpdateTagAsync(string accessToken, int id, UpdateTagRequest request);
        Task<ApiResponse?> DeleteTagAsync(string accessToken, int id);

        // ── Attribute Templates ───────────────────────────────────
        Task<ApiResponse<List<AttributeTemplateDto>>?> GetAttributeTemplatesAsync(string accessToken);
        Task<ApiResponse<AttributeTemplateDto>?> GetAttributeTemplateByIdAsync(string accessToken, int id);
        Task<ApiResponse<AttributeTemplateDto>?> CreateAttributeTemplateAsync(string accessToken, CreateAttributeTemplateRequest request);
        Task<ApiResponse<AttributeTemplateDto>?> UpdateAttributeTemplateAsync(string accessToken, int id, UpdateAttributeTemplateRequest request);
        Task<ApiResponse?> DeleteAttributeTemplateAsync(string accessToken, int id);


        // ── Users (Identity module) ──────────────────────────────────
        Task<ApiResponse<PagedResult<UserDto>>?> GetUsersAsync(
            string accessToken, int page, int pageSize,
            string? search = null, string? role = null, string? status = null,
            string sortBy = "createdat", string sortDirection = "desc");

        Task<ApiResponse<UserDto>?> GetUserByIdAsync(
            string accessToken, Guid id);

        Task<ApiResponse?> SuspendUserAsync(
            string accessToken, Guid id, string reason);

        Task<ApiResponse?> ActivateUserAsync(
            string accessToken, Guid id);

        Task<ApiResponse?> DeleteUserAsync(
            string accessToken, Guid id);

        // ── Sellers (Identity module) ────────────────────────────────
        Task<ApiResponse<PagedResult<SellerDto>>?> GetSellersAsync(
            string accessToken, int page, int pageSize,
            string? search = null, string? sellerStatus = null,
            string sortBy = "createdat", string sortDirection = "desc");

        Task<ApiResponse<SellerDto>?> GetSellerByIdAsync(
            string accessToken, Guid id);

        Task<ApiResponse?> ApproveSellerAsync(
            string accessToken, Guid id);

        Task<ApiResponse?> RejectSellerAsync(
            string accessToken, Guid id, string reason);

        Task<ApiResponse?> SuspendSellerAsync(
            string accessToken, Guid id, string reason);

        Task<ApiResponse?> ActivateSellerAsync(
            string accessToken, Guid id);
    }
}