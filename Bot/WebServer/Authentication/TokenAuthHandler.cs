using System.Security.Claims;
using System.Text.Encodings.Web;
using Bot.configs;
using Bot.Database;
using Bot.Database.Dao;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace Bot.WebServer.Authentication;

public class TokenAuthOptions : AuthenticationSchemeOptions
{
  public const string DefaultSchemeName= "TokenAuthenticationScheme";
  public string  TokenHeaderName{get;set;}= "X-AUTH-TOKEN";
}

public class TokenAuthHandler : AuthenticationHandler<TokenAuthOptions>
{
  public TokenAuthHandler(IOptionsMonitor<TokenAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
    : base(options, logger, encoder, clock) { }
  
  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    
    if (!Request.Headers.ContainsKey(Options.TokenHeaderName))
    {
      Response.StatusCode = 401;
      return Task.FromResult(AuthenticateResult.Fail($"Missing Header For Token: {Options.TokenHeaderName}"));
    }
    
    var token = Request.Headers[Options.TokenHeaderName];

    
    try
    {
      
      if (!AuthUtils.CheckToken(token))
      {
        Response.StatusCode = 401;
        return Task.FromResult(AuthenticateResult.Fail("Invalid Token Header"));
      }
    }
    catch (MySqlException e)
    {
      Console.WriteLine(e);
      Response.StatusCode = 501;
      return Task.FromResult(AuthenticateResult.Fail("Internal Server Error"));
    }
    
    var claims = new[] {
      new Claim("name", GlobalConfig.WebConfig!.AuthUsr),
      new Claim(ClaimTypes.Role, "Admin")
    };
    var id = new ClaimsIdentity(claims, Scheme.Name);
    var principal = new ClaimsPrincipal(id);
    var ticket = new AuthenticationTicket(principal, Scheme.Name);
    return Task.FromResult(AuthenticateResult.Success(ticket));
  }
}