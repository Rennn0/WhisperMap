using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using XatiCraft.Guards;

namespace XatiCraft.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : ControllerBase
{
    private readonly ILogger<SessionController> _logger;
    private readonly IDataProtector _protector;

    public SessionController(IDataProtectionProvider dataProtectionProvider, ILogger<SessionController> logger)
    {
        _protector = dataProtectionProvider.CreateProtector(AuthGuard.GetProtectionPurpose());
        _logger = logger;
    }

    [HttpGet]
    public IActionResult InitSession()
    {
        if (!HttpContext.Request.Headers.TryGetValue("x-public-ip", out StringValues forwardHeader))
            return Unauthorized();

        string ip = forwardHeader.ToString().Split(',')[0].Trim();
        _logger.LogInformation("public ip {ip}", ip);
        string protectedData =
            _protector.Protect(JsonSerializer.Serialize(new SessionData(ip, Guid.NewGuid().ToString("N"))));
        CookieOptions cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
            IsEssential = true
        };
        HttpContext.Response.Cookies.Append("session", protectedData, cookieOptions);
        return NoContent();
    }
}