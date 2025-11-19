using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using XatiCraft.Guards;

namespace XatiCraft.Controllers;

/// <summary>
/// </summary>
[ApiController]
[Route("s")]
[ApiKeyGuard]
public class SessionController : ControllerBase
{
    private readonly Security _aspProtector;
    private readonly ILogger<SessionController> _logger;

    /// <summary>
    /// </summary>
    /// <param name="securities"></param>
    /// <param name="logger"></param>
    public SessionController(IEnumerable<Security> securities, ILogger<SessionController> logger)
    {
        _aspProtector = securities.First(s => s is AspDataProtector);
        _logger = logger;
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
        CookieOptions cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.Now.AddDays(1),
            IsEssential = true
        };
        HttpContext.Response.Cookies.Append(AuthGuard.SessionCookie, protectedData, cookieOptions);
        return NoContent();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet("me")]
    public IActionResult Me([FromServices] UserGuard userGuard)
    {
        if (!userGuard.TryGetUserInfo(out UserInfo? userInfo)) return Unauthorized();
        return Ok(userInfo);
    }
}