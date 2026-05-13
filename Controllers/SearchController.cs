using Microsoft.AspNetCore.Mvc;
using MusicServer.Models;
using MusicServer.Services;

namespace MusicServer.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ILogger<SearchController> _logger;
    private readonly MusicService _musicService;
    private readonly AuthService _authService;

    public SearchController(
        ILogger<SearchController> logger,
        MusicService musicService,
        AuthService authService)
    {
        _logger = logger;
        _musicService = musicService;
        _authService = authService;
    }

    /// <summary>
    /// Search for songs, artists, albums, or genres
    /// </summary>
    [HttpGet]
    public IActionResult Search([FromQuery] string q, [FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "Query parameter 'q' is required" });

        _logger.LogInformation($"Search requested: {q}");
        var songs = _musicService.SearchSongs(q);
        
        return Ok(new
        {
            query = q,
            songCount = songs.Count,
            songs = songs
        });
    }
}
