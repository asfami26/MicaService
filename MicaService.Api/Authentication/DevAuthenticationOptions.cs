using Microsoft.AspNetCore.Authentication;

namespace MicaService.Api.Authentication;

public sealed class DevAuthenticationOptions : AuthenticationSchemeOptions
{
    public string? UserName { get; set; }
}
