using AuthSession.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AuthSession.Handlers;

sealed class BlackColorHandler : AuthorizationHandler<DressColorRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DressColorRequirement requirement)
    {
        var colorClaim = context.User.Claims.FirstOrDefault(c => c.Type == "color");
        if (colorClaim != null && colorClaim.Value.Equals("black", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}