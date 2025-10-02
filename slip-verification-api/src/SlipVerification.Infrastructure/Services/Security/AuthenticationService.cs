using Microsoft.Extensions.Logging;
using SlipVerification.Application.DTOs.Auth;
using SlipVerification.Application.Interfaces;
using SlipVerification.Application.Interfaces.Repositories;
using SlipVerification.Domain.Entities;

namespace SlipVerification.Infrastructure.Services.Security;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtService,
        IPasswordHasher passwordHasher,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
    {
        // Find user
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent user: {Email}", request.Email);
            return AuthenticationResult.Failed("Invalid email or password");
        }

        // Check if account is locked
        if (user.IsLockedOut)
        {
            if (user.LockoutEnd > DateTime.UtcNow)
            {
                _logger.LogWarning("Login attempt for locked account: {Email}", request.Email);
                return AuthenticationResult.Failed($"Account is locked until {user.LockoutEnd}");
            }

            // Unlock account if lockout period has passed
            user.IsLockedOut = false;
            user.FailedLoginAttempts = 0;
            await _userRepository.UpdateAsync(user);
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Increment failed login attempts
            user.FailedLoginAttempts++;

            // Lock account after 5 failed attempts
            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLockedOut = true;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(30);
                _logger.LogWarning("Account locked due to multiple failed login attempts: {Email}",
                    request.Email);
            }

            await _userRepository.UpdateAsync(user);
            return AuthenticationResult.Failed("Invalid email or password");
        }

        // Check if email is verified
        if (!user.EmailVerified)
        {
            return AuthenticationResult.Failed("Please verify your email address");
        }

        // Reset failed login attempts
        if (user.FailedLoginAttempts > 0)
        {
            user.FailedLoginAttempts = 0;
            await _userRepository.UpdateAsync(user);
        }

        // Get user roles
        var roles = await _userRepository.GetUserRolesAsync(user.Id);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id);

        // Save refresh token
        await _refreshTokenRepository.AddAsync(refreshToken);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User logged in successfully: {Email}", request.Email);

        return AuthenticationResult.CreateSuccess(accessToken, refreshToken.Token, user);
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            _logger.LogWarning("Invalid refresh token used");
            return AuthenticationResult.Failed("Invalid refresh token");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired refresh token used");
            return AuthenticationResult.Failed("Refresh token expired");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null)
        {
            return AuthenticationResult.Failed("User not found");
        }

        // Get user roles
        var roles = await _userRepository.GetUserRolesAsync(user.Id);

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken(user.Id);

        // Revoke old refresh token
        storedToken.IsActive = false;
        storedToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(storedToken);

        // Save new refresh token
        await _refreshTokenRepository.AddAsync(newRefreshToken);

        _logger.LogInformation("Tokens refreshed for user: {UserId}", user.Id);

        return AuthenticationResult.CreateSuccess(newAccessToken, newRefreshToken.Token, user);
    }

    public async Task<bool> LogoutAsync(Guid userId)
    {
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
        _logger.LogInformation("User logged out: {UserId}", userId);
        return true;
    }

    public async Task<RegistrationResult> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return RegistrationResult.Failed("Email already registered");
        }

        // Check if username already exists
        existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            return RegistrationResult.Failed("Username already taken");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            FullName = request.FullName,
            PasswordHash = passwordHash,
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            Role = Domain.Enums.UserRole.User
        };

        await _userRepository.AddAsync(user);

        // Generate email verification token
        var verificationToken = Guid.NewGuid().ToString();
        await _userRepository.SaveEmailVerificationTokenAsync(user.Id, verificationToken);

        // TODO: Send verification email

        _logger.LogInformation("New user registered: {Email}", request.Email);

        return RegistrationResult.CreateSuccess(user);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        // Hash new password
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Password changed for user: {UserId}", userId);

        return true;
    }
}
