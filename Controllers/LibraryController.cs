using Microsoft.AspNetCore.Mvc;

namespace MusicServer.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly ILogger<LibraryController> _logger;
    private readonly MusicService _musicService;
    private readonly AuthService _authService;

    public LibraryController(
        ILogger<LibraryController> logger,
        MusicService musicService,
        AuthService authService)
    {
        _logger = logger;
        _musicService = musicService;
        _authService = authService;
    }

    /// <summary>
    /// Get all songs
    /// </summary>
    [HttpGet("songs")]
    public IActionResult GetAllSongs([FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation("All songs requested");
        var songs = _musicService.GetAllSongs();
        return Ok(new { count = songs.Count, songs });
    }

    /// <summary>
    /// Get song by ID
    /// </summary>
    [HttpGet("songs/{id}")]
    public IActionResult GetSongById(string id, [FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        var song = _musicService.GetSongById(id);
        if (song == null)
            return NotFound();

        return Ok(song);
    }

    /// <summary>
    /// Get all artists
    /// </summary>
    [HttpGet("artists")]
    public IActionResult GetArtists([FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation("All artists requested");
        var artists = _musicService.GetAllArtists();
        return Ok(new { count = artists.Count, artists });
    }

    /// <summary>
    /// Get all albums
    /// </summary>
    [HttpGet("albums")]
    public IActionResult GetAlbums([FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation("All albums requested");
        var albums = _musicService.GetAllAlbums();
        return Ok(new { count = albums.Count, albums });
    }

    /// <summary>
    /// Get all genres
    /// </summary>
    [HttpGet("genres")]
    public IActionResult GetGenres([FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation("All genres requested");
        var genres = _musicService.GetAllGenres();
        return Ok(new { count = genres.Count, genres });
    }

    /// <summary>
    /// Get library statistics
    /// </summary>
    [HttpGet("stats")]
    public IActionResult GetStats([FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation("Library statistics requested");
        var totalSongs = _musicService.GetTotalSongCount();
        var totalDuration = _musicService.GetTotalDuration();
        var artists = _musicService.GetAllArtists().Count;
        var albums = _musicService.GetAllAlbums().Count;
        var genres = _musicService.GetAllGenres().Count;

        return Ok(new
        {
            totalSongs,
            totalDurationMs = totalDuration,
            totalDurationHours = totalDuration / 3600000.0,
            uniqueArtists = artists,
            uniqueAlbums = albums,
            uniqueGenres = genres
        });
    }

    /// <summary>
    /// Refresh music library (rescan directory)
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshLibrary([FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation("Library refresh requested");
        await _musicService.RefreshLibraryAsync();
        
        return Ok(new
        {
            message = "Library refreshed successfully",
            totalSongs = _musicService.GetTotalSongCount()
        });
    }
}
