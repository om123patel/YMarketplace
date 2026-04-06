using AutoMapper;
using Payments.Application.DTOs.CommissionRules;
using Payments.Application.Interfaces;
using Payments.Application.Services.Interface;
using Payments.Domain.Entities;
using Shared.Application.Models;

namespace Payments.Application.Services
{
    public class CommissionRuleService : ICommissionRuleService
    {
        private readonly ICommissionRuleRepository _repo;
        private readonly IPaymentsUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommissionRuleService(
            ICommissionRuleRepository repo,
            IPaymentsUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<CommissionRuleDto>>> GetAllAsync(
            CancellationToken ct = default)
        {
            var rules = await _repo.GetAllAsync(ct);
            return Result<IEnumerable<CommissionRuleDto>>.Success(
                _mapper.Map<IEnumerable<CommissionRuleDto>>(rules));
        }

        public async Task<Result<CommissionRuleDto>> GetByIdAsync(
            int id, CancellationToken ct = default)
        {
            var rule = await _repo.GetByIdAsync(id, ct);
            if (rule is null || rule.IsDeleted)
                return Result<CommissionRuleDto>.Failure(
                    $"Commission rule {id} not found.", "COMMISSION_RULE_NOT_FOUND");

            return Result<CommissionRuleDto>.Success(
                _mapper.Map<CommissionRuleDto>(rule));
        }

        public async Task<Result<decimal>> CalculateCommissionAsync(
            decimal amount, int? categoryId,
            CancellationToken ct = default)
        {
            CommissionRule? rule = categoryId.HasValue
                ? await _repo.GetByCategoryIdAsync(categoryId.Value, ct)
                : null;

            rule ??= await _repo.GetDefaultAsync(ct);

            var rate = rule?.RatePercent ?? 0m;
            var commission = Math.Round(amount * rate / 100m, 2);

            return Result<decimal>.Success(commission);
        }

        public async Task<Result<CommissionRuleDto>> CreateAsync(
            CreateCommissionRuleDto dto,
            Guid adminId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Result<CommissionRuleDto>.Failure(
                    "Rule name is required.", "VALIDATION_FAILED");

            var rule = CommissionRule.Create(
                dto.Name, dto.RatePercent, adminId, dto.CategoryId);

            await _repo.AddAsync(rule, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CommissionRuleDto>.Success(
                _mapper.Map<CommissionRuleDto>(rule));
        }

        public async Task<Result<CommissionRuleDto>> UpdateAsync(
            int id, UpdateCommissionRuleDto dto,
            Guid adminId, CancellationToken ct = default)
        {
            var rule = await _repo.GetByIdAsync(id, ct);
            if (rule is null || rule.IsDeleted)
                return Result<CommissionRuleDto>.Failure(
                    $"Commission rule {id} not found.", "COMMISSION_RULE_NOT_FOUND");

            try { rule.Update(dto.Name, dto.RatePercent, dto.CategoryId, adminId); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result<CommissionRuleDto>.Failure(ex.Message, ex.Code); }

            _repo.Update(rule);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CommissionRuleDto>.Success(
                _mapper.Map<CommissionRuleDto>(rule));
        }

        public async Task<Result> ActivateAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var rule = await _repo.GetByIdAsync(id, ct);
            if (rule is null || rule.IsDeleted)
                return Result.Failure(
                    $"Commission rule {id} not found.", "COMMISSION_RULE_NOT_FOUND");

            rule.Activate(adminId);
            _repo.Update(rule);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> DeactivateAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var rule = await _repo.GetByIdAsync(id, ct);
            if (rule is null || rule.IsDeleted)
                return Result.Failure(
                    $"Commission rule {id} not found.", "COMMISSION_RULE_NOT_FOUND");

            rule.Deactivate(adminId);
            _repo.Update(rule);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var rule = await _repo.GetByIdAsync(id, ct);
            if (rule is null || rule.IsDeleted)
                return Result.Failure(
                    $"Commission rule {id} not found.", "COMMISSION_RULE_NOT_FOUND");

            rule.SoftDelete(adminId);
            _repo.Update(rule);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}