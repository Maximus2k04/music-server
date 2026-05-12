using Microsoft.AspNetCore.Mvc;

namespace MusicServer.Controllers;

[ApiController]
[Route("api/playlist")]
public class PlaylistController : ControllerBase
{
    private readonly ILogger<PlaylistController> _logger;
    private readonly PlaylistService _playlistService;
    private readonly AuthService _authService;

    public PlaylistController(
        ILogger<PlaylistController> logger,
        PlaylistService playlistService,
        AuthService authService)
    {
        _logger = logger;
        _playlistService = playlistService;
        _authService = authService;
    }

    /// <summary>
    /// Get master playlist with all songs
    /// </summary>
    [HttpGet("all")]
    public IActionResult GetMasterPlaylist([FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation("Master playlist requested");
        var m3u = _playlistService.GenerateMasterPlaylist();
        return File(System.Text.Encoding.UTF8.GetBytes(m3u), "application/vnd.apple.mpegurl", "all-songs.m3u");
    }

    /// <summary>
    /// Get artists directory (browsable in VLC)
    /// </summary>
    [HttpGet("artists")]
    public IActionResult GetArtistsPlaylist([FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation("Artists directory requested");
        var m3u = _playlistService.GenerateArtistDirectory();
        return File(System.Text.Encoding.UTF8.GetBytes(m3u), "application/vnd.apple.mpegurl", "artists.m3u");
    }

    /// <summary>
    /// Get albums directory (browsable in VLC)
    /// </summary>
    [HttpGet("albums")]
    public IActionResult GetAlbumsPlaylist([FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation("Albums directory requested");
        var m3u = _playlistService.GenerateAlbumDirectory();
        return File(System.Text.Encoding.UTF8.GetBytes(m3u), "application/vnd.apple.mpegurl", "albums.m3u");
    }

    /// <summary>
    /// Get genres directory (browsable in VLC)
    /// </summary>
    [HttpGet("genres")]
    public IActionResult GetGenresPlaylist([FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation("Genres directory requested");
        var m3u = _playlistService.GenerateGenreDirectory();
        return File(System.Text.Encoding.UTF8.GetBytes(m3u), "application/vnd.apple.mpegurl", "genres.m3u");
    }

    /// <summary>
    /// Get playlist for specific artist
    /// </summary>
    [HttpGet("artist/{artist}")]
    public IActionResult GetArtistPlaylist(string artist, [FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation($"Artist playlist requested: {artist}");
        var m3u = _playlistService.GenerateArtistPlaylist(artist);
        return File(System.Text.Encoding.UTF8.GetBytes(m3u), "application/vnd.apple.mpegurl", $"{artist}.m3u");
    }

    /// <summary>
    /// Get playlist for specific album
    /// </summary>
    [HttpGet("album/{album}")]
    public IActionResult GetAlbumPlaylist(string album, [FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation($"Album playlist requested: {album}");
        var m3u = _playlistService.GenerateAlbumPlaylist(album);
        return File(System.Text.Encoding.UTF8.GetBytes(m3u), "application/vnd.apple.mpegurl", $"{album}.m3u");
    }

    /// <summary>
    /// Get playlist for specific genre
    /// </summary>
    [HttpGet("genre/{genre}")]
    public IActionResult GetGenrePlaylist(string genre, [FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation($"Genre playlist requested: {genre}");
        var m3u = _playlistService.GenerateGenrePlaylist(genre);
        return File(System.Text.Encoding.UTF8.GetBytes(m3u), "application/vnd.apple.mpegurl", $"{genre}.m3u");
    }
}
