using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace ClipsService.Auth;

public class HasAnyPermRequirement : IAuthorizationRequirement
{
    public List<string> Permissions { get; }
    public string Issuer { get; }

    public HasAnyPermRequirement(List<string> permissions, string issuer)
    {
        if (permissions == null || !permissions.Any()) throw new ArgumentNullException(nameof(permissions));
        else Permissions = permissions;
        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
    }
}
