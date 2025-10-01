namespace SlipVerification.Application.Interfaces;

/// <summary>
/// Interface for JWT token service
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for a user
    /// </summary>
    string GenerateToken(Guid userId, string username, string role);
    
    /// <summary>
    /// Generates a refresh token
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates a JWT token
    /// </summary>
    bool ValidateToken(string token);
    
    /// <summary>
    /// Gets user ID from token
    /// </summary>
    Guid? GetUserIdFromToken(string token);
}
