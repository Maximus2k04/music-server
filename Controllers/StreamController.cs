using Microsoft.AspNetCore.Mvc;
using MusicServer.Models;
using MusicServer.Services;

namespace MusicServer.Controllers;

[ApiController]
[Route("api/stream")]
public class StreamController : ControllerBase
{
    private readonly ILogger<StreamController> _logger;
    private readonly StreamingService _streamingService;
    private readonly AuthService _authService;

    public StreamController(
        ILogger<StreamController> logger,
        StreamingService streamingService,
        AuthService authService)
    {
        _logger = logger;
        _streamingService = streamingService;
        _authService = authService;
    }

    /// <summary>
    /// Stream audio file
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> StreamAudio(string id, [FromQuery] string? token)
    {
        if (!_authService.ValidateToken(token))
            return Unauthorized();

        _logger.LogInformation($"Stream requested for song: {id}");
        await _streamingService.StreamAudioAsync(Response, id, Request);
        return new EmptyResult();
    }
}
