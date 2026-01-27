using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace MicaService.Api.Authentication;

public sealed class WindowsClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
        {
            if (!identity.HasClaim(c => c.Type == "username"))
            {
                var name = identity.Name;
                var normalized = NormalizeWindowsUsername(name) ?? name ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    identity.AddClaim(new Claim("username", normalized));
                }
            }
        }

        return Task.FromResult(principal);
    }

    private static string? NormalizeWindowsUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var trimmed = username.Trim();
        var slashIndex = trimmed.LastIndexOf('\\');
        if (slashIndex >= 0 && slashIndex + 1 < trimmed.Length)
            trimmed = trimmed[(slashIndex + 1)..];

        var atIndex = trimmed.IndexOf('@');
        if (atIndex > 0)
            trimmed = trimmed[..atIndex];

        return trimmed;
    }
}
