using Ba7besh.Application.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Ba7besh.Api.Helpers;

public static class AuthorizationExtensions
{
    public static AuthorizationPolicyBuilder RequireRole(this AuthorizationPolicyBuilder builder, UserRole role)
    {
        return builder.RequireRole(Enum.GetName(typeof(UserRole), role) ?? throw new InvalidOperationException());
    }
}