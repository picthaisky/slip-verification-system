namespace SlipVerification.Application.Interfaces;

/// <summary>
/// Interface for user permission service
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    /// Checks if user has a specific permission
    /// </summary>
    Task<bool> UserHasPermissionAsync(Guid userId, string permission);
    
    /// <summary>
    /// Gets all permissions for a user
    /// </summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
}
