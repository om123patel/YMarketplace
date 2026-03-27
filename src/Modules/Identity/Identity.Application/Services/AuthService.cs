// Identity.Application/Services/AuthService.cs
using Identity.Application.DTOs;
using Identity.Application.DTOs.User;
using Identity.Application.Interfaces;
using Identity.Application.Services.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Events;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Identity.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly ITokenService _tokenService;
        private readonly IIdentityUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(
            IUserRepository userRepo,
            IRefreshTokenRepository refreshTokenRepo,
            ITokenService tokenService,
            IIdentityUnitOfWork unitOfWork,
            IEventBus eventBus,
            IPasswordHasher passwordHasher)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _passwordHasher = passwordHasher;
        }

        // ── Register ───────────────────────────────────────────
        public async Task<Result<AuthResultDto>> RegisterAsync(
            RegisterDto dto, CancellationToken ct = default)
        {
            // 1. Validate role — public registration only allows Buyer/Seller
            if (!Enum.TryParse<UserRole>(dto.Role, out var role) ||
                role == UserRole.Admin)
            {
                return Result<AuthResultDto>.Failure(
                    "Invalid role.", "INVALID_ROLE");
            }

            // 2. Check email uniqueness
            if (await _userRepo.ExistsByEmailAsync(dto.Email, ct))
                return Result<AuthResultDto>.Failure(
                    $"Email '{dto.Email}' is already registered.", "EMAIL_EXISTS");

            // 3. Hash password
            var hash = _passwordHasher.HashPassword(dto.Password);

            // 4. Create user entity
            var user = User.Create(
                firstName: dto.FirstName,
                lastName: dto.LastName,
                email: dto.Email,
                passwordHash: hash,
                role: role,
                createdBy: Guid.Empty, // self-registered
                phoneNumber: dto.PhoneNumber);

            await _userRepo.AddAsync(user, ct);

            // 5. Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshRaw = _tokenService.GenerateRefreshToken();
            var refreshExpiry = _tokenService.GetRefreshTokenExpiry();

            var refreshToken = RefreshToken.Create(
                userId: user.Id,
                token: refreshRaw,
                expiresAt: refreshExpiry,
                createdBy: user.Id);

            await _refreshTokenRepo.AddAsync(refreshToken, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // 6. Publish domain event
            await _eventBus.PublishAsync(
                new UserRegisteredEvent(user.Id, user.Email, role.ToString()), ct);

            return Result<AuthResultDto>.Success(BuildAuthResult(
                user, accessToken, refreshRaw, refreshExpiry));
        }

        // ── Login ──────────────────────────────────────────────
        public async Task<Result<AuthResultDto>> LoginAsync(
            LoginDto dto, string? ipAddress = null, CancellationToken ct = default)
        {
            // 1. Find user
            var user = await _userRepo.GetByEmailAsync(dto.Email, ct);
            if (user is null)
                return Result<AuthResultDto>.Failure(
                    "Invalid email or password.", "INVALID_CREDENTIALS");

            // 2. Check lockout
            if (user.IsLockedOut())
                return Result<AuthResultDto>.Failure(
                    "Account is temporarily locked due to too many failed attempts. Try again in 15 minutes.",
                    "ACCOUNT_LOCKED");

            // 3. Check status
            if (user.Status == Identity.Domain.Enums.UserStatus.Suspended)
                return Result<AuthResultDto>.Failure(
                    "Your account has been suspended. Contact support.",
                    "ACCOUNT_SUSPENDED");

            if (user.Status == Identity.Domain.Enums.UserStatus.Inactive)
                return Result<AuthResultDto>.Failure(
                    "Your account is inactive.", "ACCOUNT_INACTIVE");

            // 4. Verify password
            if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                user.RecordFailedLogin();
                _userRepo.Update(user);
                await _unitOfWork.SaveChangesAsync(ct);

                return Result<AuthResultDto>.Failure(
                    "Invalid email or password.", "INVALID_CREDENTIALS");
            }

            // 5. Record success
            user.RecordSuccessfulLogin();
            _userRepo.Update(user);

            // 6. Revoke all old refresh tokens for this user (rotation)
            await _refreshTokenRepo.RevokeAllForUserAsync(
                user.Id, "New login", ct);

            // 7. Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshRaw = _tokenService.GenerateRefreshToken();
            var refreshExpiry = _tokenService.GetRefreshTokenExpiry();

            var refreshToken = RefreshToken.Create(
                userId: user.Id,
                token: refreshRaw,
                expiresAt: refreshExpiry,
                createdBy: user.Id,
                createdByIp: ipAddress);

            await _refreshTokenRepo.AddAsync(refreshToken, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<AuthResultDto>.Success(BuildAuthResult(
                user, accessToken, refreshRaw, refreshExpiry));
        }

        // ── Refresh ────────────────────────────────────────────
        public async Task<Result<AuthResultDto>> RefreshTokenAsync(
            string refreshToken, string? ipAddress = null, CancellationToken ct = default)
        {
            var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken, ct);

            if (token is null || !token.IsActive())
                return Result<AuthResultDto>.Failure(
                    "Invalid or expired refresh token.", "INVALID_REFRESH_TOKEN");

            var user = await _userRepo.GetByIdAsync(token.UserId, ct);
            if (user is null || user.IsDeleted)
                return Result<AuthResultDto>.Failure(
                    "User not found.", "USER_NOT_FOUND");

            // Rotate: revoke old token, issue new one
            var newRefreshRaw = _tokenService.GenerateRefreshToken();
            var newRefreshExpiry = _tokenService.GetRefreshTokenExpiry();

            token.Revoke("Replaced by refresh", newRefreshRaw);
            _refreshTokenRepo.Update(token);

            var newRefreshToken = RefreshToken.Create(
                userId: user.Id,
                token: newRefreshRaw,
                expiresAt: newRefreshExpiry,
                createdBy: user.Id,
                createdByIp: ipAddress);

            await _refreshTokenRepo.AddAsync(newRefreshToken, ct);

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<AuthResultDto>.Success(BuildAuthResult(
                user, newAccessToken, newRefreshRaw, newRefreshExpiry));
        }

        // ── Revoke ─────────────────────────────────────────────
        public async Task<Result> RevokeTokenAsync(
            string refreshToken, Guid userId, CancellationToken ct = default)
        {
            var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken, ct);

            if (token is null || token.UserId != userId)
                return Result.Failure("Token not found.", "INVALID_REFRESH_TOKEN");

            if (!token.IsActive())
                return Result.Failure(
                    "Token is already revoked or expired.", "INVALID_REFRESH_TOKEN");

            token.Revoke("Logged out by user");
            _refreshTokenRepo.Update(token);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ── Helper ─────────────────────────────────────────────
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