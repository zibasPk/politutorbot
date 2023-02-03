using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Bot.configs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bot.WebServer.Authentication;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
    ) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (authHeader != null && authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Basic ".Length..].Trim();
            
            var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var credentials = credentialString.Split(':');
            if (credentials[0] == GlobalConfig.WebConfig!.AuthUsr && credentials[1] == GlobalConfig.WebConfig!.AuthPsw)
            {
                var claims = new[] { new Claim("name", credentials[0]), new Claim(ClaimTypes.Role, "Admin") };
                var identity = new ClaimsIdentity(claims, "Basic");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
            }

            Response.StatusCode = 401;
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }
        
        Response.StatusCode = 401;
        return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
    }
}