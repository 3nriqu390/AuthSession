using Microsoft.AspNetCore.Authorization;

namespace AuthSession.Requirements;

sealed class UserNameRequirement(params string[] allowedUserNames) : IAuthorizationRequirement
{
    public IReadOnlyCollection<string> AllowedUserNames { get; } = allowedUserNames;
}