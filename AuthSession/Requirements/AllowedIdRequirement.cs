using Microsoft.AspNetCore.Authorization;

namespace AuthSession.Requirements;

sealed class AllowedIdRequirement(params int[] allowedIds) : IAuthorizationRequirement
{
    public IReadOnlyCollection<int> AllowedIds { get; } = allowedIds;
}
