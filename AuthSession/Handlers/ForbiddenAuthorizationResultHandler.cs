using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace AuthSession.Handlers;

sealed class ForbiddenAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);

        if (authorizeResult.Forbidden &&
            context.Response.StatusCode == StatusCodes.Status403Forbidden &&
            !context.Response.HasStarted)
        {
            await context.Response.WriteAsJsonAsync(authorizeResult.AuthorizationFailure?.FailureReasons.FirstOrDefault()?.Message);
        }
    }
}
