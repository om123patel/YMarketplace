using AutoMapper;
using Orders.Application.DTOs.Dispute;
using Orders.Application.Interfaces;
using Orders.Application.Services.Interface;
using Shared.Application.Models;
using Shared.Domain.Exceptions;

namespace Orders.Application.Services
{
    public class DisputeService : IDisputeService
    {
        private readonly IDisputeRepository _disputeRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IOrdersUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DisputeService(
            IDisputeRepository disputeRepo,
            IOrderRepository orderRepo,
            IOrdersUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _disputeRepo = disputeRepo;
            _orderRepo = orderRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<DisputeDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var dispute = await _disputeRepo.GetByIdAsync(id, ct);
            if (dispute is null)
                return Result<DisputeDto>.Failure($"Dispute {id} not found.", "DISPUTE_NOT_FOUND");

            return Result<DisputeDto>.Success(_mapper.Map<DisputeDto>(dispute));
        }

        public async Task<Result<PagedList<DisputeDto>>> GetPagedAsync(
            int page, int pageSize, string? status, CancellationToken ct = default)
        {
            var paged = await _disputeRepo.GetPagedAsync(page, pageSize, status, ct);
            var mapped = new PagedList<DisputeDto>(
                _mapper.Map<List<DisputeDto>>(paged.Items),
                paged.Page, paged.PageSize, paged.TotalCount);
            return Result<PagedList<DisputeDto>>.Success(mapped);
        }

        public async Task<Result<DisputeDto>> OpenDisputeAsync(
            Guid orderId, OpenDisputeDto dto, Guid buyerId, CancellationToken ct = default)
        {
            var order = await _orderRepo.GetByIdWithDetailsAsync(orderId, ct);
            if (order is null || order.IsDeleted)
                return Result<DisputeDto>.Failure($"Order {orderId} not found.", "ORDER_NOT_FOUND");

            try
            {
                var dispute = order.OpenDispute(buyerId, dto.Reason, dto.Evidence, buyerId);
                _orderRepo.Update(order);
                await _unitOfWork.SaveChangesAsync(ct);
                return Result<DisputeDto>.Success(_mapper.Map<DisputeDto>(dispute));
            }
            catch (DomainException ex)
            {
                return Result<DisputeDto>.Failure(ex.Message, ex.Code);
            }
        }

        public async Task<Result> SubmitSellerResponseAsync(
            Guid disputeId, string response, Guid sellerId, CancellationToken ct = default)
        {
            var dispute = await _disputeRepo.GetByIdAsync(disputeId, ct);
            if (dispute is null)
                return Result.Failure($"Dispute {disputeId} not found.", "DISPUTE_NOT_FOUND");

            try { dispute.SubmitSellerResponse(response, sellerId); }
            catch (DomainException ex) { return Result.Failure(ex.Message, ex.Code); }

            _disputeRepo.Update(dispute);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> ResolveAsync(
            Guid disputeId, ResolveDisputeDto dto, Guid adminId, CancellationToken ct = default)
        {
            var dispute = await _disputeRepo.GetByIdAsync(disputeId, ct);
            if (dispute is null)
                return Result.Failure($"Dispute {disputeId} not found.", "DISPUTE_NOT_FOUND");

            try { dispute.Resolve(adminId, dto.Resolution); }
            catch (DomainException ex) { return Result.Failure(ex.Message, ex.Code); }

            _disputeRepo.Update(dispute);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> EscalateAsync(
            Guid disputeId, string note, Guid adminId, CancellationToken ct = default)
        {
            var dispute = await _disputeRepo.GetByIdAsync(disputeId, ct);
            if (dispute is null)
                return Result.Failure($"Dispute {disputeId} not found.", "DISPUTE_NOT_FOUND");

            try { dispute.Escalate(adminId, note); }
            catch (DomainException ex) { return Result.Failure(ex.Message, ex.Code); }

            _disputeRepo.Update(dispute);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
    }

}
