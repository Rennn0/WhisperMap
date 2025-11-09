using Microsoft.AspNetCore.Mvc;
using XatiCraft.Objects;

namespace XatiCraft.Controllers;

/// <summary>
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly SystemHealthMonitor _healthMonitor;
    private readonly ILogger<HealthController> _logger;

    /// <summary>
    /// </summary>
    /// <param name="healthMonitor"></param>
    /// <param name="logger"></param>
    public HealthController(SystemHealthMonitor healthMonitor, ILogger<HealthController> logger)
    {
        _healthMonitor = healthMonitor;
        _logger = logger;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("ok");
        return Ok(new
        {
            _healthMonitor.Start, _healthMonitor.Uptime
        });
    }
}