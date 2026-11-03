using AuthSession.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AuthSession.Handlers;

sealed class AllowedIdAuthorizationHandler : AuthorizationHandler<AllowedIdRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AllowedIdRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext &&
            int.TryParse(httpContext.Request.RouteValues["id"]?.ToString(), out var id) &&
            requirement.AllowedIds.Contains(id))
        {
            context.Succeed(requirement);
            return;
        }
        
        context.Fail(new AuthorizationFailureReason(this, "Id is not allowed"));
    }
}
