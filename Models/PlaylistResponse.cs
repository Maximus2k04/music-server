namespace MusicServer.Models;

public class PlaylistResponse
{
    public string Name { get; set; } = string.Empty;
    public int SongCount { get; set; }
    public long TotalDuration { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string M3UContent { get; set; } = string.Empty;
}
