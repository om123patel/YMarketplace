using AutoMapper;
using FluentValidation;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Services.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Exceptions;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Identity.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;
        private readonly IValidator<RegisterUserDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly IValidator<UpdateProfileDto> _updateProfileValidator;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordService passwordService,
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            IMapper mapper,
            IValidator<RegisterUserDto> registerValidator,
            IValidator<LoginDto> loginValidator,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IValidator<UpdateProfileDto> updateProfileValidator)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordService = passwordService;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _mapper = mapper;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _changePasswordValidator = changePasswordValidator;
            _updateProfileValidator = updateProfileValidator;
        }

        // ══════════════════════════════════════════════════════
        // REGISTER
        // ══════════════════════════════════════════════════════
        public async Task<Result<AuthResponseDto>> RegisterAsync(
            RegisterUserDto dto, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _registerValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<AuthResponseDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Check email uniqueness
            if (await _userRepository.EmailExistsAsync(dto.Email, ct))
                return Result<AuthResponseDto>.Failure(
                    $"An account with email '{dto.Email}' already exists.",
                    "EMAIL_EXISTS");

            // 3. Parse role
            if (!Enum.TryParse<UserRole>(dto.Role, out var role))
                return Result<AuthResponseDto>.Failure(
                    $"Invalid role '{dto.Role}'.", "INVALID_ROLE");

            // 4. Hash password
            var passwordHash = _passwordService.HashPassword(dto.Password);

            // 5. Create user via factory
            var user = User.Create(
                firstName: dto.FirstName,
                lastName: dto.LastName,
                email: dto.Email,
                passwordHash: passwordHash,
                role: role,
                phoneNumber: dto.PhoneNumber);

            // 6. Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenString = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = _tokenService.GetRefreshTokenExpiry();

            user.AddRefreshToken(refreshTokenString, refreshTokenExpiry);

            // 7. Persist
            await _userRepository.AddAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // 8. Publish domain events
            await PublishAndClearEventsAsync(user, ct);

            // 9. Return auth response
            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiry(),
                RefreshTokenExpiresAt = refreshTokenExpiry,
                User = _mapper.Map<UserDto>(user)
            });
        }

        // ══════════════════════════════════════════════════════
        // LOGIN
        // ══════════════════════════════════════════════════════
        public async Task<Result<AuthResponseDto>> LoginAsync(
            LoginDto dto, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _loginValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<AuthResponseDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Find user by email — include refresh tokens
            var user = await _userRepository.GetByIdWithTokensAsync(
                (await _userRepository.GetByEmailAsync(dto.Email, ct))?.Id
                ?? Guid.Empty, ct);

            if (user is null)
            {
                // Don't reveal whether email exists — generic message
                return Result<AuthResponseDto>.Failure(
                    "Email or password is incorrect.", "INVALID_CREDENTIALS");
            }

            // 3. Check account lock
            if (user.IsLocked)
                return Result<AuthResponseDto>.Failure(
                    $"Account is temporarily locked due to multiple failed attempts. " +
                    $"Please try again later.",
                    "ACCOUNT_LOCKED");


            string hashPass = _passwordService.HashPassword(dto.Password);
            // 4. Verify password
            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
            {
                user.RecordFailedLogin();
                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(ct);
                return Result<AuthResponseDto>.Failure(
                    "Email or password is incorrect.", "INVALID_CREDENTIALS");
            }

            // 5. Record successful login
            try
            {
                user.RecordLogin();
            }
            catch (IdentityException ex)
            {
                return Result<AuthResponseDto>.Failure(ex.Message, ex.Code);
            }

            // 6. Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenString = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = _tokenService.GetRefreshTokenExpiry();

            user.AddRefreshToken(
                refreshTokenString,
                refreshTokenExpiry,
                dto.IpAddress);

            // 7. Persist
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);

            // 8. Publish domain events
            await PublishAndClearEventsAsync(user, ct);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiry(),
                RefreshTokenExpiresAt = refreshTokenExpiry,
                User = _mapper.Map<UserDto>(user)
            });
        }

        // ══════════════════════════════════════════════════════
        // REFRESH TOKEN
        // ══════════════════════════════════════════════════════
        public async Task<Result<AuthResponseDto>> RefreshTokenAsync(
            RefreshTokenDto dto, CancellationToken ct = default)
        {
            // 1. Find user by refresh token
            var user = await _userRepository.GetByRefreshTokenAsync(dto.RefreshToken, ct);
            if (user is null)
                return Result<AuthResponseDto>.Failure(
                    "Invalid refresh token.", "INVALID_REFRESH_TOKEN");

            // 2. Get the specific token
            var refreshToken = user.GetActiveRefreshToken(dto.RefreshToken);
            if (refreshToken is null)
                return Result<AuthResponseDto>.Failure(
                    "Refresh token is expired or revoked.", "INVALID_REFRESH_TOKEN");

            // 3. Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshTokenString = _tokenService.GenerateRefreshToken();
            var newRefreshTokenExpiry = _tokenService.GetRefreshTokenExpiry();

            // 4. Revoke old, add new
            refreshToken.Revoke("Replaced by new token.", newRefreshTokenString);
            user.AddRefreshToken(
                newRefreshTokenString,
                newRefreshTokenExpiry,
                dto.IpAddress);

            // 5. Persist
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString,
                AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiry(),
                RefreshTokenExpiresAt = newRefreshTokenExpiry,
                User = _mapper.Map<UserDto>(user)
            });
        }

        // ══════════════════════════════════════════════════════
        // REVOKE TOKEN
        // ══════════════════════════════════════════════════════
        public async Task<Result> RevokeTokenAsync(
            string refreshToken, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken, ct);
            if (user is null)
                return Result.Failure("Invalid refresh token.", "INVALID_REFRESH_TOKEN");

            user.RevokeRefreshToken(refreshToken, "Revoked by user.");
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ══════════════════════════════════════════════════════
        // GET CURRENT USER
        // ══════════════════════════════════════════════════════
        public async Task<Result<UserDto>> GetCurrentUserAsync(
            Guid userId, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null || user.IsDeleted)
                return Result<UserDto>.Failure(
                    $"User {userId} not found.", "USER_NOT_FOUND");

            return Result<UserDto>.Success(_mapper.Map<UserDto>(user));
        }

        // ══════════════════════════════════════════════════════
        // UPDATE PROFILE
        // ══════════════════════════════════════════════════════
        public async Task<Result<UserDto>> UpdateProfileAsync(
            Guid userId, UpdateProfileDto dto, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _updateProfileValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<UserDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Find user
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null || user.IsDeleted)
                return Result<UserDto>.Failure(
                    $"User {userId} not found.", "USER_NOT_FOUND");

            // 3. Update via domain method
            user.UpdateProfile(
                dto.FirstName,
                dto.LastName,
                dto.PhoneNumber,
                dto.AvatarUrl,
                userId);

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<UserDto>.Success(_mapper.Map<UserDto>(user));
        }

        // ══════════════════════════════════════════════════════
        // CHANGE PASSWORD
        // ══════════════════════════════════════════════════════
        public async Task<Result> ChangePasswordAsync(
            Guid userId, ChangePasswordDto dto, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _changePasswordValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Find user
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user is null || user.IsDeleted)
                return Result.Failure($"User {userId} not found.", "USER_NOT_FOUND");

            // 3. Verify current password
            if (!_passwordService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                return Result.Failure(
                    "Current password is incorrect.", "INVALID_CURRENT_PASSWORD");

            // 4. Hash new password and update
            var newPasswordHash = _passwordService.HashPassword(dto.NewPassword);
            user.ChangePassword(newPasswordHash, userId);

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        // ══════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ══════════════════════════════════════════════════════
        private async Task PublishAndClearEventsAsync(
            User user, CancellationToken ct)
        {
            foreach (var domainEvent in user.DomainEvents)
                await _eventBus.PublishAsync(domainEvent, ct);

            user.ClearDomainEvents();
        }
    }

}
