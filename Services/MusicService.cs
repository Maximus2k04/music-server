using TagFile = TagLib.File;
using MusicServer.Models;

namespace MusicServer.Services;

public class MusicService
{
    private readonly ILogger<MusicService> _logger;
    private readonly string _musicPath;
    private List<Song> _songs = new();
    private Dictionary<string, Song> _songsById = new();
    private FileSystemWatcher? _watcher;
    private readonly object _lockObject = new();

    private static readonly string[] SupportedExtensions = { ".mp3", ".flac", ".wav", ".ogg", ".m4a" };

    public MusicService(ILogger<MusicService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _musicPath = configuration["MusicPath"] ?? "/sdcard/Music";
        _logger.LogInformation($"Music service initialized with path: {_musicPath}");
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing music library...");
        await ScanMusicDirectoryAsync(_musicPath);
        SetupFileWatcher();
        _logger.LogInformation($"Music library initialized with {_songs.Count} songs");
    }

    private async Task ScanMusicDirectoryAsync(string dirPath)
    {
        try
        {
            if (!Directory.Exists(dirPath))
            {
                _logger.LogWarning($"Music directory does not exist: {dirPath}");
                return;
            }

            lock (_lockObject)
            {
                _songs.Clear();
                _songsById.Clear();
            }

            var files = Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories)
                .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            _logger.LogInformation($"Found {files.Count} audio files to process");

            foreach (var filePath in files)
            {
                try
                {
                    var song = ExtractSongMetadata(filePath);
                    lock (_lockObject)
                    {
                        _songs.Add(song);
                        _songsById[song.Id] = song;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing file: {filePath}");
                }
            }

            _logger.LogInformation($"Successfully loaded {_songs.Count} songs");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning music directory");
        }
    }

    private Song ExtractSongMetadata(string filePath)
    {
        var file = TagFile.Create(filePath);
        var tag = file.Tag;
        var properties = file.Properties;

        var song = new Song
        {
            Title = string.IsNullOrWhiteSpace(tag.Title) ? Path.GetFileNameWithoutExtension(filePath) : tag.Title,
            Artist = string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer,
            Album = string.IsNullOrWhiteSpace(tag.Album) ? "Unknown Album" : tag.Album,
            Genre = string.IsNullOrWhiteSpace(tag.FirstGenre) ? "Unknown Genre" : tag.FirstGenre,
            Year = (int)tag.Year,
            Duration = (long)properties.Duration.TotalMilliseconds,
            FilePath = filePath,
            FileSize = new FileInfo(filePath).Length,
            FileExtension = Path.GetExtension(filePath).ToLower()
        };

        return song;
    }

    private void SetupFileWatcher()
    {
        try
        {
            _watcher = new FileSystemWatcher(_musicPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true
            };

            _watcher.Created += OnMusicFileCreated;
            _watcher.Deleted += OnMusicFileDeleted;
            _watcher.Renamed += OnMusicFileRenamed;
            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation("File watcher initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up file watcher");
        }
    }

    private void OnMusicFileCreated(object sender, FileSystemEventArgs e)
    {
        if (SupportedExtensions.Contains(Path.GetExtension(e.FullPath).ToLower()))
        {
            Task.Run(async () =>
            {
                await Task.Delay(500); // Wait for file to be written
                try
                {
                    var song = ExtractSongMetadata(e.FullPath);
                    lock (_lockObject)
                    {
                        _songs.Add(song);
                        _songsById[song.Id] = song;
                    }
                    _logger.LogInformation($"Added new song: {song.Title}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error adding new song: {e.FullPath}");
                }
            });
        }
    }

    private void OnMusicFileDeleted(object sender, FileSystemEventArgs e)
    {
        var songToRemove = _songs.FirstOrDefault(s => s.FilePath == e.FullPath);
        if (songToRemove != null)
        {
            lock (_lockObject)
            {
                _songs.Remove(songToRemove);
                _songsById.Remove(songToRemove.Id);
            }
            _logger.LogInformation($"Removed song: {songToRemove.Title}");
        }
    }

    private void OnMusicFileRenamed(object sender, RenamedEventArgs e)
    {
        OnMusicFileDeleted(sender, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(e.OldFullPath)!, Path.GetFileName(e.OldFullPath)));
        OnMusicFileCreated(sender, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(e.FullPath)!, Path.GetFileName(e.FullPath)));
    }

    public List<Song> GetAllSongs()
    {
        lock (_lockObject)
        {
            return _songs.OrderBy(s => s.Artist).ThenBy(s => s.Album).ThenBy(s => s.Title).ToList();
        }
    }

    public Song? GetSongById(string id)
    {
        lock (_lockObject)
        {
            return _songsById.TryGetValue(id, out var song) ? song : null;
        }
    }

    public List<Song> SearchSongs(string query)
    {
        var lowerQuery = query.ToLower();
        lock (_lockObject)
        {
            return _songs.Where(s =>
                s.Title.ToLower().Contains(lowerQuery) ||
                s.Artist.ToLower().Contains(lowerQuery) ||
                s.Album.ToLower().Contains(lowerQuery)
            ).ToList();
        }
    }

    public List<string> GetAllArtists()
    {
        lock (_lockObject)
        {
            return _songs.Select(s => s.Artist).Distinct().OrderBy(a => a).ToList();
        }
    }

    public List<Song> GetSongsByArtist(string artist)
    {
        lock (_lockObject)
        {
            return _songs.Where(s => s.Artist.Equals(artist, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.Album)
                .ThenBy(s => s.Title)
                .ToList();
        }
    }

    public List<string> GetAllAlbums()
    {
        lock (_lockObject)
        {
            return _songs.Select(s => s.Album).Distinct().OrderBy(a => a).ToList();
        }
    }

    public List<Song> GetSongsByAlbum(string album)
    {
        lock (_lockObject)
        {
            return _songs.Where(s => s.Album.Equals(album, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.Title)
                .ToList();
        }
    }

    public List<string> GetAllGenres()
    {
        lock (_lockObject)
        {
            return _songs.Select(s => s.Genre).Distinct().OrderBy(g => g).ToList();
        }
    }

    public List<Song> GetSongsByGenre(string genre)
    {
        lock (_lockObject)
        {
            return _songs.Where(s => s.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.Artist)
                .ThenBy(s => s.Album)
                .ThenBy(s => s.Title)
                .ToList();
        }
    }

    public int GetTotalSongCount()
    {
        lock (_lockObject)
        {
            return _songs.Count;
        }
    }

    public long GetTotalDuration()
    {
        lock (_lockObject)
        {
            return _songs.Sum(s => s.Duration);
        }
    }

    public async Task RefreshLibraryAsync()
    {
        _logger.LogInformation("Refreshing music library...");
        await ScanMusicDirectoryAsync(_musicPath);
    }
}
