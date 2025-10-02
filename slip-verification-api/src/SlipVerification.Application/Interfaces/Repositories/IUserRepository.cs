using SlipVerification.Domain.Entities;

namespace SlipVerification.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task AddUserToRoleAsync(Guid userId, string role);
    Task SaveEmailVerificationTokenAsync(Guid userId, string token);
    Task<bool> VerifyEmailAsync(string token);
}

/// <summary>
/// Repository interface for RefreshToken entity
/// </summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllUserTokensAsync(Guid userId);
}
