namespace MusicServer.Services;

public class StreamingService
{
    private readonly ILogger<StreamingService> _logger;
    private readonly MusicService _musicService;

    public StreamingService(ILogger<StreamingService> logger, MusicService musicService)
    {
        _logger = logger;
        _musicService = musicService;
    }

    public async Task StreamAudioAsync(HttpResponse response, string songId, HttpRequest request)
    {
        try
        {
            var song = _musicService.GetSongById(songId);
            if (song == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                await response.WriteAsJsonAsync(new { error = "Song not found" });
                return;
            }

            if (!File.Exists(song.FilePath))
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                await response.WriteAsJsonAsync(new { error = "Audio file not found" });
                return;
            }

            var fileInfo = new FileInfo(song.FilePath);
            var fileStream = File.OpenRead(song.FilePath);

            // Set response headers
            response.ContentType = GetContentType(song.FileExtension);
            response.Headers.Add("Accept-Ranges", "bytes");
            response.Headers.Add("Content-Disposition", $"inline; filename=\"{song.Title}{song.FileExtension}\"");

            // Handle range requests (for seeking in VLC)
            if (request.Headers.ContainsKey("Range"))
            {
                await HandleRangeRequest(response, fileStream, fileInfo, request);
            }
            else
            {
                response.ContentLength = fileInfo.Length;
                await fileStream.CopyToAsync(response.Body);
            }

            await fileStream.FlushAsync();
            fileStream.Close();
            fileStream.Dispose();

            _logger.LogInformation($"Streamed song: {song.Title}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error streaming audio: {songId}");
            if (!response.HasStarted)
            {
                response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }

    private async Task HandleRangeRequest(HttpResponse response, FileStream fileStream, FileInfo fileInfo, HttpRequest request)
    {
        var rangeHeader = request.Headers["Range"].ToString();
        if (string.IsNullOrEmpty(rangeHeader) || !rangeHeader.StartsWith("bytes="))
        {
            response.ContentLength = fileInfo.Length;
            await fileStream.CopyToAsync(response.Body);
            return;
        }

        var range = rangeHeader.Substring(6).Split('-');
        long start = 0;
        long end = fileInfo.Length - 1;

        if (!string.IsNullOrEmpty(range[0]))
            start = long.Parse(range[0]);

        if (!string.IsNullOrEmpty(range[1]))
            end = long.Parse(range[1]);

        long length = end - start + 1;

        response.StatusCode = StatusCodes.Status206PartialContent;
        response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileInfo.Length}");
        response.ContentLength = length;

        fileStream.Seek(start, SeekOrigin.Begin);
        await fileStream.CopyToAsync(response.Body, (int)length);
    }

    private string GetContentType(string fileExtension)
    {
        return fileExtension.ToLower() switch
        {
            ".mp3" => "audio/mpeg",
            ".flac" => "audio/flac",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".m4a" => "audio/mp4",
            _ => "audio/mpeg"
        };
    }
}
