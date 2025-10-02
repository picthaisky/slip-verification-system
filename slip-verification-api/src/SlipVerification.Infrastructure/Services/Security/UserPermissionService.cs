using SlipVerification.Application.Authorization;
using SlipVerification.Application.Interfaces;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Infrastructure.Services.Security;

/// <summary>
/// User permission service implementation with role-based permissions
/// </summary>
public class UserPermissionService : IUserPermissionService
{
    private static readonly Dictionary<string, string[]> RolePermissions = new()
    {
        [Roles.Admin] = new[]
        {
            Permissions.ViewSlips,
            Permissions.UploadSlips,
            Permissions.DeleteSlips,
            Permissions.VerifySlips,
            Permissions.ViewOrders,
            Permissions.CreateOrders,
            Permissions.UpdateOrders,
            Permissions.DeleteOrders,
            Permissions.ViewUsers,
            Permissions.ManageUsers,
            Permissions.ViewReports,
            Permissions.ExportReports
        },
        [Roles.Manager] = new[]
        {
            Permissions.ViewSlips,
            Permissions.UploadSlips,
            Permissions.VerifySlips,
            Permissions.ViewOrders,
            Permissions.CreateOrders,
            Permissions.UpdateOrders,
            Permissions.ViewUsers,
            Permissions.ViewReports,
            Permissions.ExportReports
        },
        [Roles.User] = new[]
        {
            Permissions.ViewSlips,
            Permissions.UploadSlips,
            Permissions.ViewOrders,
            Permissions.CreateOrders
        },
        [Roles.Guest] = new[]
        {
            Permissions.ViewSlips,
            Permissions.ViewOrders
        }
    };

    public Task<bool> UserHasPermissionAsync(Guid userId, string permission)
    {
        // In a real application, you would fetch the user's role from the database
        // For now, we'll return a placeholder value
        // This should be implemented based on the actual user role retrieval logic
        return Task.FromResult(false);
    }

    public Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        // In a real application, you would fetch the user's role from the database
        // For now, we'll return an empty collection
        // This should be implemented based on the actual user role retrieval logic
        return Task.FromResult(Enumerable.Empty<string>());
    }

    /// <summary>
    /// Gets permissions for a role
    /// </summary>
    public static IEnumerable<string> GetPermissionsForRole(UserRole role)
    {
        var roleName = role.ToString();
        return RolePermissions.TryGetValue(roleName, out var permissions)
            ? permissions
            : Array.Empty<string>();
    }

    /// <summary>
    /// Checks if a role has a specific permission
    /// </summary>
    public static bool RoleHasPermission(UserRole role, string permission)
    {
        var permissions = GetPermissionsForRole(role);
        return permissions.Contains(permission);
    }
}
