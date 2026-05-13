using Microsoft.AspNetCore.Mvc;
using MusicServer.Services;

namespace MusicServer.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly MusicService _musicService;

    public HealthController(
        ILogger<HealthController> logger,
        MusicService musicService)
    {
        _logger = logger;
        _musicService = musicService;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        var songCount = _musicService.GetTotalSongCount();
        var status = songCount > 0 ? "healthy" : "no-music";

        return Ok(new
        {
            status,
            timestamp = DateTime.UtcNow,
            songCount,
            version = "1.0.0"
        });
    }
}
