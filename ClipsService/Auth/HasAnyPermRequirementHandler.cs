using Microsoft.AspNetCore.Authorization;

namespace ClipsService.Auth;

public class HasAnyPermRequirementHandler : AuthorizationHandler<HasAnyPermRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasAnyPermRequirement requirement)
    {
        // Check if permissions exist
        if (!context.User.HasClaim(c => c.Type == "permissions" && c.Issuer == requirement.Issuer))
            return Task.CompletedTask;

        var permissions = context.User.Claims.Where(c => c.Type == "permissions");
        if (!permissions.Any())
            return Task.CompletedTask;

        // Succeed if the permission array contains the required scope
        if (permissions.Any(s => requirement.Permissions.Any(r => r == s.Value)))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
