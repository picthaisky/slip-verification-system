using System.Security.Claims;
using SlipVerification.Domain.Entities;

namespace SlipVerification.Application.Interfaces;

/// <summary>
/// Interface for JWT token service
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for a user with roles
    /// </summary>
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    
    /// <summary>
    /// Generates a refresh token for a user
    /// </summary>
    RefreshToken GenerateRefreshToken(Guid userId);
    
    /// <summary>
    /// Validates a JWT token and returns ClaimsPrincipal
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);
    
    /// <summary>
    /// Gets user ID from an expired token (for refresh token flow)
    /// </summary>
    Guid? GetUserIdFromExpiredToken(string token);
    
    /// <summary>
    /// Generates a JWT token for a user (legacy method)
    /// </summary>
    [Obsolete("Use GenerateAccessToken instead")]
    string GenerateToken(Guid userId, string username, string role);
    
    /// <summary>
    /// Generates a refresh token (legacy method)
    /// </summary>
    [Obsolete("Use GenerateRefreshToken(Guid userId) instead")]
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates a JWT token (legacy method)
    /// </summary>
    [Obsolete("Use ValidateToken that returns ClaimsPrincipal instead")]
    bool ValidateToken(string token, out bool isValid);
    
    /// <summary>
    /// Gets user ID from token (legacy method)
    /// </summary>
    [Obsolete("Use GetUserIdFromExpiredToken instead")]
    Guid? GetUserIdFromToken(string token);
}
