namespace MusicServer.Models;

public class SearchResult
{
    public List<Song> Songs { get; set; } = new();
    public List<Artist> Artists { get; set; } = new();
    public List<Album> Albums { get; set; } = new();
    public List<Genre> Genres { get; set; } = new();
}
