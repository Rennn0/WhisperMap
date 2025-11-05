using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace XatiCraft.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : ControllerBase
{
    [HttpGet]
    public IActionResult InitSession([FromServices] ILogger<SessionController> logger)
    {
        string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        logger.LogInformation("remote ip {ip}", ip);
        if (HttpContext.Request.Headers.TryGetValue("x-forwarded-for", out StringValues forwardHeader))
        {
            ip = forwardHeader.ToString().Split(',')[0].Trim();
            logger.LogInformation("forwarded ip {ip}", ip);
        }

        if (HttpContext.Request.Headers.TryGetValue("x-real-ip", out StringValues realIpHeader))
        {
            ip = realIpHeader.ToString().Split(',')[0].Trim();
            logger.LogInformation("real ip {ip}", ip);
        }

        logger.LogCritical(string.Join('_', HttpContext.Request.Headers.Keys));
        logger.LogCritical(string.Join('_', HttpContext.Request.Headers.Values));

        string sessionId = Guid.NewGuid().ToString("N");
        CookieOptions cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
            IsEssential = true
        };
        HttpContext.Response.Cookies.Append("client_ip", ip, cookieOptions);
        HttpContext.Response.Cookies.Append("session_id", sessionId, cookieOptions);
        return Ok(new { sessionId });
    }
}