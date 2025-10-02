using SlipVerification.Application.DTOs.Auth;

namespace SlipVerification.Application.Interfaces;

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    Task<AuthenticationResult> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// Logs out a user by revoking their refresh tokens
    /// </summary>
    Task<bool> LogoutAsync(Guid userId);
    
    /// <summary>
    /// Registers a new user
    /// </summary>
    Task<RegistrationResult> RegisterAsync(RegisterRequest request);
    
    /// <summary>
    /// Changes user password
    /// </summary>
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
