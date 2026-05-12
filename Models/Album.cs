namespace MusicServer.Models;

public class Album
{
    public string Name { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<Song> Songs { get; set; } = new();
    public int SongCount => Songs.Count;
    public long TotalDuration => Songs.Sum(s => s.Duration);
}
