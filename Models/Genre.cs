namespace MusicServer.Models;

public class Genre
{
    public string Name { get; set; } = string.Empty;
    public List<Song> Songs { get; set; } = new();
    public int SongCount => Songs.Count;
    public long TotalDuration => Songs.Sum(s => s.Duration);
}
