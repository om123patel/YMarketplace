using Orders.Application.DTOs.Dispute;
using Shared.Application.Models;

namespace Orders.Application.Services.Interface
{
    public interface IDisputeService
    {
        Task<Result<DisputeDto>> OpenDisputeAsync(
            Guid orderId, OpenDisputeDto dto, Guid buyerId, CancellationToken ct = default);
        Task<Result> SubmitSellerResponseAsync(
            Guid disputeId, string response, Guid sellerId, CancellationToken ct = default);
        Task<Result> ResolveAsync(
            Guid disputeId, ResolveDisputeDto dto, Guid adminId, CancellationToken ct = default);
        Task<Result> EscalateAsync(
            Guid disputeId, string note, Guid adminId, CancellationToken ct = default);
        Task<Result<PagedList<DisputeDto>>> GetPagedAsync(
            int page, int pageSize, string? status, CancellationToken ct = default);
        Task<Result<DisputeDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    }
}
