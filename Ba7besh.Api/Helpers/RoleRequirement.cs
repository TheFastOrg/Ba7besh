using Ba7besh.Application.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Ba7besh.Api.Helpers;

public class RoleRequirement(UserRole requiredRole) : IAuthorizationRequirement
{
    public UserRole RequiredRole { get; } = requiredRole;
}

public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        RoleRequirement requirement)
    {
        if (context.User.IsInRole(requirement.RequiredRole.ToString()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}