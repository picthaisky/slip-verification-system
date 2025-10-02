using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SlipVerification.Application.Authorization;
using SlipVerification.Application.Interfaces;

namespace SlipVerification.API.Authorization;

/// <summary>
/// Authorization handler for permission-based authorization
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserPermissionService _permissionService;

    public PermissionAuthorizationHandler(IUserPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return;
        }

        var hasPermission = await _permissionService.UserHasPermissionAsync(
            userGuid,
            requirement.Permission
        );

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
