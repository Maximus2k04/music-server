using MusicServer.Models;

namespace MusicServer.Services;

public class PlaylistService
{
    private readonly ILogger<PlaylistService> _logger;
    private readonly MusicService _musicService;
    private readonly string _baseUrl;
    private readonly string? _authToken;

    public PlaylistService(ILogger<PlaylistService> logger, MusicService musicService, IConfiguration configuration)
    {
        _logger = logger;
        _musicService = musicService;
        _baseUrl = configuration["BaseUrl"] ?? "http://localhost:5000";
        _authToken = configuration["AuthToken"];
    }

    public string GenerateMasterPlaylist()
    {
        var songs = _musicService.GetAllSongs();
        return GenerateM3UPlaylist("All Music", songs);
    }

    public string GenerateArtistPlaylist(string artist)
    {
        var songs = _musicService.GetSongsByArtist(artist);
        return GenerateM3UPlaylist($"Artist: {artist}", songs);
    }

    public string GenerateAlbumPlaylist(string album)
    {
        var songs = _musicService.GetSongsByAlbum(album);
        return GenerateM3UPlaylist($"Album: {album}", songs);
    }

    public string GenerateGenrePlaylist(string genre)
    {
        var songs = _musicService.GetSongsByGenre(genre);
        return GenerateM3UPlaylist($"Genre: {genre}", songs);
    }

    public string GenerateArtistDirectory()
    {
        var artists = _musicService.GetAllArtists();
        var m3u = new System.Text.StringBuilder();
        m3u.AppendLine("#EXTM3U");
        m3u.AppendLine("#PLAYLIST: Artists Directory");
        m3u.AppendLine();

        foreach (var artist in artists)
        {
            var artistSongs = _musicService.GetSongsByArtist(artist);
            var totalDuration = artistSongs.Sum(s => s.Duration) / 1000;
            var query = string.IsNullOrEmpty(_authToken) ? string.Empty : $"?token={_authToken}";
            var playlistUrl = $"{_baseUrl}/api/playlist/artist/{Uri.EscapeDataString(artist)}{query}";
            
            m3u.AppendLine($"#EXTINF:{totalDuration},{artist} ({artistSongs.Count} songs)");
            m3u.AppendLine(playlistUrl);
            m3u.AppendLine();
        }

        return m3u.ToString();
    }

    public string GenerateAlbumDirectory()
    {
        var albums = _musicService.GetAllAlbums();
        var m3u = new System.Text.StringBuilder();
        m3u.AppendLine("#EXTM3U");
        m3u.AppendLine("#PLAYLIST: Albums Directory");
        m3u.AppendLine();

        foreach (var album in albums)
        {
            var albumSongs = _musicService.GetSongsByAlbum(album);
            var totalDuration = albumSongs.Sum(s => s.Duration) / 1000;
            var query = string.IsNullOrEmpty(_authToken) ? string.Empty : $"?token={_authToken}";
            var playlistUrl = $"{_baseUrl}/api/playlist/album/{Uri.EscapeDataString(album)}{query}";
            
            m3u.AppendLine($"#EXTINF:{totalDuration},{album} ({albumSongs.Count} songs)");
            m3u.AppendLine(playlistUrl);
            m3u.AppendLine();
        }

        return m3u.ToString();
    }

    public string GenerateGenreDirectory()
    {
        var genres = _musicService.GetAllGenres();
        var m3u = new System.Text.StringBuilder();
        m3u.AppendLine("#EXTM3U");
        m3u.AppendLine("#PLAYLIST: Genres Directory");
        m3u.AppendLine();

        foreach (var genre in genres)
        {
            var genreSongs = _musicService.GetSongsByGenre(genre);
            var totalDuration = genreSongs.Sum(s => s.Duration) / 1000;
            var query = string.IsNullOrEmpty(_authToken) ? string.Empty : $"?token={_authToken}";
            var playlistUrl = $"{_baseUrl}/api/playlist/genre/{Uri.EscapeDataString(genre)}{query}";
            
            m3u.AppendLine($"#EXTINF:{totalDuration},{genre} ({genreSongs.Count} songs)");
            m3u.AppendLine(playlistUrl);
            m3u.AppendLine();
        }

        return m3u.ToString();
    }

    private string GenerateM3UPlaylist(string playlistName, List<Song> songs)
    {
        var m3u = new System.Text.StringBuilder();
        m3u.AppendLine("#EXTM3U");
        m3u.AppendLine($"#PLAYLIST: {playlistName}");
        m3u.AppendLine($"#EXTINF: -1,{playlistName} ({songs.Count} songs, {songs.Sum(s => s.Duration) / 1000}s)");
        m3u.AppendLine();

        foreach (var song in songs)
        {
            m3u.AppendLine(song.ToM3UEntry(_baseUrl, _authToken));
        }

        return m3u.ToString();
    }
}
