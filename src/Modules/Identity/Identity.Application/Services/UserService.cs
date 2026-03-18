using AutoMapper;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Services.Interfaces;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Identity.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PagedList<UserDto>>> GetPagedAsync(
            int page, int pageSize,
            string? role, string? status, string? search,
            CancellationToken ct = default)
        {
            var paged = await _userRepo.GetPagedAsync(
                page, pageSize, role, status, search, ct);

            var mapped = new PagedList<UserDto>(
                paged.Items.Select(u => _mapper.Map<UserDto>(u)).ToList(),
                paged.Page, paged.PageSize, paged.TotalCount);

            return Result<PagedList<UserDto>>.Success(mapped);
        }

        public async Task<Result<UserDto>> GetByIdAsync(
            Guid id, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(id, ct);
            if (user is null || user.IsDeleted)
                return Result<UserDto>.Failure(
                    $"User {id} not found.", "USER_NOT_FOUND");

            return Result<UserDto>.Success(_mapper.Map<UserDto>(user));
        }

        public async Task<Result> SuspendAsync(
            Guid id, Guid adminId, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(id, ct);
            if (user is null || user.IsDeleted)
                return Result.Failure($"User {id} not found.", "USER_NOT_FOUND");

            try { user.Suspend(adminId); }
            catch (Shared.Domain.Exceptions.DomainException ex)
            { return Result.Failure(ex.Message, ex.Code); }

            _userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<Result> ActivateAsync(
            Guid id, Guid adminId, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(id, ct);
            if (user is null || user.IsDeleted)
                return Result.Failure($"User {id} not found.", "USER_NOT_FOUND");

            user.Activate(adminId);
            _userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}