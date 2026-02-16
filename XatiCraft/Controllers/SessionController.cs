using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using XatiCraft.ApiContracts;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Impl;

namespace XatiCraft.Controllers;

/// <summary>
/// </summary>
[ApiController]
[Route("s")]
[ApiKeyGuard]
public class SessionController : ControllerBase
{
    private readonly Security _aspProtector;
    private readonly CookieOptions _cookieOptions;
    private readonly ILogger<SessionController> _logger;

    /// <summary>
    /// </summary>
    /// <param name="securities"></param>
    /// <param name="logger"></param>
    public SessionController(IEnumerable<Security> securities, ILogger<SessionController> logger)
    {
        _aspProtector = securities.First(s => s is AspDataProtector);
        _logger = logger;
        _cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.Now.AddDays(7),
            IsEssential = true
        };
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult InitSession()
    {
        if (!HttpContext.Request.Headers.TryGetValue(AuthGuard.InitHeader, out StringValues forwardHeader))
            return Unauthorized();

        string ip = forwardHeader.ToString().Split(',')[0].Trim();
        _logger.LogInformation("public ip {ip}", ip);
        string protectedData =
            _aspProtector.Pack(JsonSerializer.Serialize(new SessionData(ip, Guid.NewGuid().ToString("N"))));
        SetSessionCookie(protectedData);
        return NoContent();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet("me")]
    public async Task<IActionResult> Me([FromServices] UserGuard userGuard,
        [FromServices] IEnumerable<IAuthorizationHandler> handlers, CancellationToken cancellationToken)
    {
        if (!userGuard.TryGetUserInfo(out UserInfo? userInfo)) return Unauthorized();
        if (userInfo is not { Uid.Length: > 0 }) return Ok(userInfo);

        AuthorizationContract contract = (AuthorizationContract)await handlers.First(h => h is GoogleAuthHandler)
            .HandleAsync(new UserInfoContext(userInfo.Uid), cancellationToken);
        userInfo.Username = contract.Username;
        userInfo.Picture = contract.ProfilePicture;

        return Ok(userInfo);
    }

    /// <summary>
    /// </summary>
    /// <param name="token"></param>
    /// <param name="provider"></param>
    /// <param name="handlers"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("gt/{provider}")]
    public async Task<IActionResult> VerifyGoogleToken(
        [FromRoute] ApplicationAuthProvider provider,
        [FromQuery(Name = "t")] string token,
        [FromServices] IEnumerable<IAuthorizationHandler> handlers, CancellationToken cancellationToken)
    {
        AuthorizationContract contract = (AuthorizationContract)await handlers.First(h => provider switch
            {
                ApplicationAuthProvider.Google => h is GoogleAuthHandler,
                ApplicationAuthProvider.Github => h is GithubAuthHandler,
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
            })
            .HandleAsync(new AuthorizationContext(token, provider), cancellationToken);
        SetUserIdCookie(contract.Uid);
        return NoContent();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet("lo")]
    public IActionResult Logout()
    {
        ClearCookie(AuthGuard.UserIdCookie);
        ClearCookie(AuthGuard.SessionCookie);
        //#NOTE invalidate session maybe
        return NoContent();
    }

    private void SetSessionCookie(string data)
    {
        HttpContext.Response.Cookies.Append(AuthGuard.SessionCookie, data, _cookieOptions);
    }


    private void SetUserIdCookie(string data)
    {
        HttpContext.Response.Cookies.Append(AuthGuard.UserIdCookie, data, _cookieOptions);
    }

    private void ClearCookie(string key)
    {
        HttpContext.Response.Cookies.Delete(key, _cookieOptions);
    }
}