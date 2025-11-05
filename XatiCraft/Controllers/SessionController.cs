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
        if (!HttpContext.Request.Headers.TryGetValue("x-public-ip", out StringValues forwardHeader))
            return Unauthorized();
        string ip = forwardHeader.ToString().Split(',')[0].Trim();
        logger.LogInformation("public ip {ip}", ip);
        string sessionId = Guid.NewGuid().ToString("N");
        CookieOptions cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
            IsEssential = true
        };
        HttpContext.Response.Cookies.Append("ip", ip, cookieOptions);
        HttpContext.Response.Cookies.Append("session_id", sessionId, cookieOptions);
        return Ok(new { sessionId });
    }
}