// src/Modules/Identity/Identity.Application/Services/CustomerAuthService.cs
using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.DTOs.Customer;
using Identity.Application.DTOs.User;
using Identity.Application.Interfaces;
using Identity.Application.Services.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Identity.Application.Services
{
    public class CustomerAuthService : ICustomerAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly ITokenService _tokenService;
        private readonly IIdentityUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<CustomerRegisterDto> _registerValidator;
        private readonly IValidator<CustomerLoginDto> _loginValidator;

        public CustomerAuthService(
            IUserRepository userRepo,
            IRefreshTokenRepository refreshTokenRepo,
            ITokenService tokenService,
            IIdentityUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IValidator<CustomerRegisterDto> registerValidator,
            IValidator<CustomerLoginDto> loginValidator)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        // ── Register ──────────────────────────────────────────────
        public async Task<Result<AuthResultDto>> RegisterAsync(
            CustomerRegisterDto dto, CancellationToken ct = default)
        {
            var validation = await _registerValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<AuthResultDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            if (await _userRepo.ExistsByEmailAsync(dto.Email, ct))
                return Result<AuthResultDto>.Failure(
                    $"Email '{dto.Email}' is already registered.", "EMAIL_EXISTS");

            var hash = _passwordHasher.HashPassword(dto.Password);

            var user = User.Create(
                firstName: dto.FirstName,
                lastName: dto.LastName,
                email: dto.Email,
                passwordHash: hash,
                role: UserRole.Buyer,
                createdBy: Guid.Empty,
                phoneNumber: dto.PhoneNumber);

            await _userRepo.AddAsync(user, ct);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshRaw = _tokenService.GenerateRefreshToken();
            var refreshExpiry = _tokenService.GetRefreshTokenExpiry();

            var refreshToken = RefreshToken.Create(
                userId: user.Id,
                token: refreshRaw,
                expiresAt: refreshExpiry,
                createdBy: user.Id);

            // Attach the tracked user entity so EF will insert User first, then RefreshToken
            refreshToken.AttachUser(user);

            await _refreshTokenRepo.AddAsync(refreshToken, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<AuthResultDto>.Success(
                BuildAuthResult(user, accessToken, refreshRaw, refreshExpiry));
        }

        // ── Login ─────────────────────────────────────────────────
        public async Task<Result<AuthResultDto>> LoginAsync(
            CustomerLoginDto dto, string? ipAddress = null,
            CancellationToken ct = default)
        {
            var validation = await _loginValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<AuthResultDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            var user = await _userRepo.GetByEmailAsync(dto.Email, ct);
            if (user is null)
                return Result<AuthResultDto>.Failure(
                    "Invalid email or password.", "INVALID_CREDENTIALS");

            // Only Buyers can log in through this endpoint
            if (user.Role != UserRole.Buyer)
                return Result<AuthResultDto>.Failure(
                    "Invalid email or password.", "INVALID_CREDENTIALS");

            if (user.IsLockedOut())
                return Result<AuthResultDto>.Failure(
                    "Account is temporarily locked. Try again in 15 minutes.",
                    "ACCOUNT_LOCKED");

            if (user.Status == UserStatus.Suspended)
                return Result<AuthResultDto>.Failure(
                    "Your account has been suspended. Contact support.",
                    "ACCOUNT_SUSPENDED");

            if (user.Status == UserStatus.Inactive)
                return Result<AuthResultDto>.Failure(
                    "Your account is inactive.", "ACCOUNT_INACTIVE");

            if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                user.RecordFailedLogin();
                _userRepo.Update(user);
                await _unitOfWork.SaveChangesAsync(ct);
                return Result<AuthResultDto>.Failure(
                    "Invalid email or password.", "INVALID_CREDENTIALS");
            }

            user.RecordSuccessfulLogin();
            _userRepo.Update(user);

            await _refreshTokenRepo.RevokeAllForUserAsync(user.Id, "New login", ct);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshRaw = _tokenService.GenerateRefreshToken();
            var refreshExpiry = _tokenService.GetRefreshTokenExpiry();

            var refreshToken = RefreshToken.Create(
                userId: user.Id,
                token: refreshRaw,
                expiresAt: refreshExpiry,
                createdBy: user.Id,
                createdByIp: ipAddress);

            // Ensure EF knows the relationship ordering by attaching the tracked user
            refreshToken.AttachUser(user);

            await _refreshTokenRepo.AddAsync(refreshToken, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<AuthResultDto>.Success(
                BuildAuthResult(user, accessToken, refreshRaw, refreshExpiry));
        }

        // ── Logout ───────────────────────────────────────────────
        public async Task<Result> LogoutAsync(
            string refreshToken, Guid userId,
            CancellationToken ct = default)
        {
            var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken, ct);

            // Token not found or belongs to a different user — treat both as invalid
            if (token is null || token.UserId != userId)
                return Result.Failure(
                    "Invalid refresh token.", "INVALID_REFRESH_TOKEN");

            // Already revoked is not an error for logout — idempotent
            if (!token.IsActive())
                return Result.Success();

            token.Revoke("Logged out by customer");
            _refreshTokenRepo.Update(token);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ── GetMe ─────────────────────────────────────────────────
        public async Task<Result<UserDto>> GetMeAsync(
            Guid userId, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);

            if (user is null || user.IsDeleted)
                return Result<UserDto>.Failure(
                    "User not found.", "USER_NOT_FOUND");

            // Guard: only Buyer accounts are served through this endpoint
            if (user.Role != UserRole.Buyer)
                return Result<UserDto>.Failure(
                    "User not found.", "USER_NOT_FOUND");

            return Result<UserDto>.Success(new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt
            });
        }

        // ── Private helper ────────────────────────────────────────
        private AuthResultDto BuildAuthResult(
            User user, string accessToken,
            string refreshRaw, DateTime refreshExpiry)
        {
            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshRaw,
                AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiry(),
                RefreshTokenExpiresAt = refreshExpiry,
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    Role = user.Role.ToString(),
                    Status = user.Status.ToString(),
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                }
            };
        }
    }
}