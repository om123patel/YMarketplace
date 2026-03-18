using AutoMapper;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Services.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Events;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Identity.Application.Services
{
    public class VendorOnboardingService : IVendorOnboardingService
    {
        private readonly IVendorApplicationRepository _appRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;

        public VendorOnboardingService(
            IVendorApplicationRepository appRepo,
            IUserRepository userRepo,
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            IMapper mapper)
        {
            _appRepo = appRepo;
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _mapper = mapper;
        }

        public async Task<Result<VendorApplicationDto>> ApplyAsync(
            Guid userId, ApplyVendorDto dto, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            if (user is null || user.IsDeleted)
                return Result<VendorApplicationDto>.Failure(
                    "User not found.", "USER_NOT_FOUND");

            // One active application per user
            var existing = await _appRepo.GetByUserIdAsync(userId, ct);
            if (existing is not null && existing.Status == VendorApplicationStatus.Pending)
                return Result<VendorApplicationDto>.Failure(
                    "You already have a pending application.", "APPLICATION_EXISTS");

            var app = VendorApplication.Create(
                userId: userId,
                storeName: dto.StoreName,
                businessType: dto.BusinessType,
                createdBy: userId,
                taxId: dto.TaxId,
                contactPhone: dto.ContactPhone,
                description: dto.Description);

            await _appRepo.AddAsync(app, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<VendorApplicationDto>.Success(
                MapToDto(app, user));
        }

        public async Task<Result<PagedList<VendorApplicationDto>>> GetPendingAsync(
            int page, int pageSize, CancellationToken ct = default)
        {
            var paged = await _appRepo.GetPagedAsync(
                page, pageSize, VendorApplicationStatus.Pending, ct);

            // Load user details for each application
            var dtos = new List<VendorApplicationDto>();
            foreach (var app in paged.Items)
            {
                var user = await _userRepo.GetByIdAsync(app.UserId, ct);
                dtos.Add(MapToDto(app, user));
            }

            return Result<PagedList<VendorApplicationDto>>.Success(
                new PagedList<VendorApplicationDto>(
                    dtos, paged.Page, paged.PageSize, paged.TotalCount));
        }

        public async Task<Result<VendorApplicationDto>> GetByIdAsync(
            Guid id, CancellationToken ct = default)
        {
            var app = await _appRepo.GetByIdAsync(id, ct);
            if (app is null)
                return Result<VendorApplicationDto>.Failure(
                    $"Application {id} not found.", "NOT_FOUND");

            var user = await _userRepo.GetByIdAsync(app.UserId, ct);
            return Result<VendorApplicationDto>.Success(MapToDto(app, user));
        }

        public async Task<Result> ApproveAsync(
            Guid applicationId, Guid adminId, CancellationToken ct = default)
        {
            var app = await _appRepo.GetByIdAsync(applicationId, ct);
            if (app is null)
                return Result.Failure("Application not found.", "NOT_FOUND");

            try { app.Approve(adminId); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result.Failure(ex.Message, ex.Code); }

            // Activate the user's seller status
            var user = await _userRepo.GetByIdAsync(app.UserId, ct);
            if (user is not null)
            {
                user.Activate(adminId);
                _userRepo.Update(user);
            }

            _appRepo.Update(app);
            await _unitOfWork.SaveChangesAsync(ct);

            await _eventBus.PublishAsync(
                new VendorApprovedEvent(app.UserId, app.Id, app.StoreName), ct);

            return Result.Success();
        }

        public async Task<Result> RejectAsync(
            Guid applicationId, Guid adminId,
            string reason, CancellationToken ct = default)
        {
            var app = await _appRepo.GetByIdAsync(applicationId, ct);
            if (app is null)
                return Result.Failure("Application not found.", "NOT_FOUND");

            try { app.Reject(adminId, reason); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result.Failure(ex.Message, ex.Code); }

            _appRepo.Update(app);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        private static VendorApplicationDto MapToDto(
            VendorApplication app, User? user)
        {
            return new VendorApplicationDto
            {
                Id = app.Id,
                UserId = app.UserId,
                ApplicantName = user?.FullName ?? "-",
                ApplicantEmail = user?.Email ?? "-",
                StoreName = app.StoreName,
                BusinessType = app.BusinessType,
                TaxId = app.TaxId,
                ContactPhone = app.ContactPhone,
                Description = app.Description,
                Status = app.Status.ToString(),
                RejectionReason = app.RejectionReason,
                ReviewedAt = app.ReviewedAt,
                CreatedAt = app.CreatedAt
            };
        }
    }
}