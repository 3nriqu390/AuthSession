using System.Globalization;
using AuthSession.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AuthSession.Handlers;

sealed class AgeAuthorizationHandler : AuthorizationHandler<AgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AgeRequirement requirement)
    {
        var ageClaim = context.User.Claims.FirstOrDefault(c => c.Type == "age");
        if (ageClaim is not null &&
            int.TryParse(ageClaim.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var age) &&
            age >= 18)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
