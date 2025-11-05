using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace XatiCraft.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : ControllerBase
{
    [HttpGet]
    public IActionResult InitSession()
    {
        string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        if (HttpContext.Request.Headers.TryGetValue("x-forwarded-for", out StringValues forwardHeader))
            ip = forwardHeader.ToString().Split(',')[0].Trim();

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