using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Payments;
using AdminPanel.Models;
using AdminPanel.Services.Interfaces;

namespace AdminPanel.Services
{
    public class TransactionApiClient : ApiClientBase, ITransactionApiClient
    {
        public TransactionApiClient(HttpClient http, ILogger<TransactionApiClient> logger)
            : base(http, logger) { }

        public Task<ApiResponse<PagedResult<TransactionListItemDto>>?> GetTransactionsAsync(
            string token, int page = 1, int pageSize = 20,
            string? status = null, string? method = null,
            string? dateFrom = null, string? dateTo = null,
            string? search = null,
            string sortBy = "createdAt", string sortDirection = "desc")
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["status"] = status,
                ["method"] = method,
                ["dateFrom"] = dateFrom,
                ["dateTo"] = dateTo,
                ["search"] = search,
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return GetAsync<ApiResponse<PagedResult<TransactionListItemDto>>>(
                $"api/admin/transactions{q}", token);
        }

        public Task<ApiResponse<TransactionDto>?> GetByIdAsync(string token, Guid id)
            => GetAsync<ApiResponse<TransactionDto>>(
                $"api/admin/transactions/{id}", token);

        public Task<ApiResponse<TransactionDto>?> GetByOrderIdAsync(
            string token, Guid orderId)
            => GetAsync<ApiResponse<TransactionDto>>(
                $"api/admin/transactions/by-order/{orderId}", token);

        public Task<ApiResponse<TransactionDto>?> RefundAsync(
            string token, Guid id, RefundTransactionRequest request)
            => PostAsync<ApiResponse<TransactionDto>>(
                $"api/admin/transactions/{id}/refund", request, token);
    }
}