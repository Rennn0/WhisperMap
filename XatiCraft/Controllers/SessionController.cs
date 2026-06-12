using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using XatiCraft.ApiContracts;
using XatiCraft.Guards;
using XatiCraft.Handlers.Impl;
using IAuthorizationHandler = XatiCraft.Handlers.Api.IAuthorizationHandler;

namespace XatiCraft.Controllers;

/// <summary>
/// </summary>
[ApiController]
[Route("s")]
// [ApiKeyGuard]
public class SessionController : ApplicationController
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
            IsEssential = true,
#if DEBUG
#else         
            Domain = "xati.org"
#endif
        };
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult InitSession()
    {
        if (
            !HttpContext.Request.Headers.TryGetValue(
                AuthGuard.IpHeader,
                out StringValues forwardHeader
            )
        )
            return Unauthorized();

        string ip = forwardHeader.ToString().Split(',')[0].Trim();
        _logger.LogInformation("public ip {ip}", ip);
        string protectedData = _aspProtector.Pack(
            JsonSerializer.Serialize(new SessionData(ip, Guid.NewGuid().ToString("N")))
        );
        AppendC(AuthGuard.SessionCookie, protectedData, _cookieOptions);
        if (User.Identity is { IsAuthenticated: true })
            AppendC(AuthGuard.UserIdCookie, User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "", _cookieOptions);
        return NoContent();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet("me")]
    [AllowAnonymous]
    public async Task<IActionResult> Me(
        [FromServices] UserGuard userGuard,
        [FromServices] IEnumerable<IAuthorizationHandler> handlers,
        CancellationToken cancellationToken
    )
    {
        if (!userGuard.TryGetUserInfo(out UserInfo? userInfo))
            return Unauthorized();

        if (userInfo is not { Uid.Length: > 0 } && User.Identity is not { IsAuthenticated: true })
            return Ok(userInfo);

        string uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? userInfo?.Uid ?? string.Empty;
        
        AuthorizationContract contract = (AuthorizationContract)
            await handlers
                .First(h => h is GoogleAuthHandler)
                .HandleAsync(new UserInfoContext(uid), cancellationToken);
        userInfo!.Username = contract.Username;
        userInfo.Picture = contract.ProfilePicture;

        return Ok(userInfo);
    }

    /// <summary>
    /// </summary>
    /// <param name="token"></param>
    /// <param name="provider"></param>
    /// <param name="handlers"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("gt/{provider}")]
    public async Task<IActionResult> VerifyToken(
        [FromRoute] ApplicationAuthProvider provider,
        [FromQuery(Name = "t")] string token,
        [FromServices] IEnumerable<IAuthorizationHandler> handlers,
        [FromServices] IWebHostEnvironment hostEnvironment,
        CancellationToken cancellationToken
    )
    {
        if (hostEnvironment.IsDevelopment()) return NoContent();
        AuthorizationContract contract = (AuthorizationContract)
            await handlers
                .First(h =>
                    provider switch
                    {
                        ApplicationAuthProvider.Google => h is GoogleAuthHandler,
                        ApplicationAuthProvider.Github => h is GithubAuthHandler,
                        _ => throw new ArgumentOutOfRangeException(
                            nameof(provider),
                            provider,
                            null
                        ),
                    }
                )
                .HandleAsync(new AuthorizationContext(token, provider), cancellationToken);
        AppendC(AuthGuard.UserIdCookie, contract.Uid, _cookieOptions);
        return NoContent();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet("lo")]
    public IActionResult Logout()
    {
        DeleteC(AuthGuard.UserIdCookie, _cookieOptions);
        DeleteC(AuthGuard.SessionCookie,_cookieOptions);
        //#NOTE invalidate session maybe
        return NoContent();
    }
}
