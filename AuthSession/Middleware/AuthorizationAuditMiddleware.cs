using Microsoft.AspNetCore.Authorization;

namespace AuthSession.Middleware;

sealed class AuthorizationAuditMiddleware(
    RequestDelegate next,
    ILogger<AuthorizationAuditMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await next(context);
            return;
        }

        await next(context);

        logger.LogInformation(
            "Request audit: {Method} {Endpoint}; user {UserName}; response status {StatusCode}",
            context.Request.Method,
            endpoint?.DisplayName ?? context.Request.Path.ToString(),
            context.User.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : "Unauthenticated",
            context.Response.StatusCode);
    }
}
