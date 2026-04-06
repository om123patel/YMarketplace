using AdminPanel.Services.Interfaces;
using Payments.Application.DTOs.Transactions;
using Shared.Application.Models;

namespace AdminPanel.Services
{
    public class TransactionApiClient : ApiClientBase, ITransactionApiClient
    {
        private readonly HttpClient _http;

        public TransactionApiClient(HttpClient http) => _http = http;

        public async Task<PagedList<TransactionListItemDto>?> GetPagedAsync(
            TransactionFilterRequest filter)
        {
            var q = BuildQuery(new()
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["search"] = search,
                ["status"] = status,
                ["sortBy"] = sortBy,
                ["sortDirection"] = sortDirection
            });
            return await _http.GetFromJsonAsync<PagedList<TransactionListItemDto>>(
                $"api/admin/transactions?{qs}");
        }

        public async Task<TransactionDto?> GetByIdAsync(Guid id)
            => await _http.GetFromJsonAsync<TransactionDto>(
                $"api/admin/transactions/{id}");

        public async Task<TransactionDto?> GetByOrderIdAsync(Guid orderId)
            => await _http.GetFromJsonAsync<TransactionDto>(
                $"api/admin/transactions/by-order/{orderId}");

        public async Task<TransactionDto?> RefundAsync(
            Guid id, RefundTransactionDto dto)
        {
            var response = await _http.PostAsJsonAsync(
                $"api/admin/transactions/{id}/refund", dto);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<TransactionDto>()
                : null;
        }
    }
}
