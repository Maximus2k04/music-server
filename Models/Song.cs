namespace MusicServer.Models;

public class Song
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Year { get; set; }
    public long Duration { get; set; } // in milliseconds
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public string FileExtension { get; set; } = string.Empty;

    public string ToM3UEntry(string baseUrl, string? authToken = null)
    {
        var durationSeconds = Duration / 1000;
        var query = string.IsNullOrEmpty(authToken) ? string.Empty : $"?token={authToken}";
        var streamUrl = $"{baseUrl}/api/stream/{Id}{query}";
        
        return $"#EXTINF:{durationSeconds},{Artist} - {Title}\n{streamUrl}";
    }
}