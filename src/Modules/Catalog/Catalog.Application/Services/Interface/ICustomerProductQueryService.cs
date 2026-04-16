// src/Modules/Catalog/Catalog.Application/Services/Interface/ICustomerProductQueryService.cs

using Catalog.Application.DTOs.Products;
using Shared.Application.Models;

namespace Catalog.Application.Services.Interface
{
    public interface ICustomerProductQueryService
    {
        Task<Result<ProductDto>> GetBySlugAsync(
            string slug, CancellationToken ct = default);

        Task<Result<IEnumerable<ProductListItemDto>>> GetFeaturedAsync(
            int limit, CancellationToken ct = default);

        Task<Result<IEnumerable<ProductListItemDto>>> GetTopSellingAsync(
            int limit, CancellationToken ct = default);

        Task<Result<IEnumerable<ProductListItemDto>>> GetRelatedAsync(
            Guid productId, int limit, CancellationToken ct = default);

        Task<Result<IEnumerable<ProductListItemDto>>> GetRecommendedAsync(
            int limit, CancellationToken ct = default);
    }
}