using AdminPanel.Dtos.Common;
using AdminPanel.Dtos.Payments;
using AdminPanel.Models;

namespace AdminPanel.Services.Interfaces
{
    public interface ITransactionApiClient
    {
        Task<ApiResponse<PagedResult<TransactionListItemDto>>?> GetTransactionsAsync(
            string token, int page = 1, int pageSize = 20,
            string? status = null, string? method = null,
            string? dateFrom = null, string? dateTo = null,
            string? search = null,
            string sortBy = "createdAt", string sortDirection = "desc");

        Task<ApiResponse<TransactionDto>?> GetByIdAsync(string token, Guid id);

        Task<ApiResponse<TransactionDto>?> GetByOrderIdAsync(string token, Guid orderId);

        Task<ApiResponse<TransactionDto>?> RefundAsync(
            string token, Guid id, RefundTransactionRequest request);
    }
}