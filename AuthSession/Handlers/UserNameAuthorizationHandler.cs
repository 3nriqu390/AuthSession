using AuthSession.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AuthSession.Handlers;

sealed class UserNameAuthorizationHandler : AuthorizationHandler<UserNameRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserNameRequirement requirement)
    {
        var userName = context.User.Identity?.Name;
        if (context.User.Identity?.IsAuthenticated == true &&
            userName is not null &&
            requirement.AllowedUserNames.Contains(userName, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}