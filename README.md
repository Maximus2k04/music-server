# 🎵 Music Server - ASP.NET Core Mobile Music Streaming

A powerful, lightweight ASP.NET Core 8 music streaming server designed to run on Android via Termux. Stream your music library to VLC Media Player from anywhere in the world with M3U playlist organization.

## Features

✅ **M3U Playlist Generation**
- Organize music by Artists, Albums, Genres
- Browsable library structure in VLC
- Master playlist with all songs

✅ **Audio Streaming**
- Support for MP3, FLAC, WAV, OGG, M4A
- Range request support (seek/resume in VLC)
- High-quality streaming

✅ **Global Access**
- Run on Android phone via Termux
- Access from any device worldwide via HTTPS
- Port forwarding compatible
- Dynamic DNS support

✅ **Real-time Updates**
- FileSystemWatcher for automatic detection
- Add/remove songs without server restart
- In-memory caching for performance

✅ **RESTful API**
- Complete REST endpoints
- Swagger/OpenAPI documentation
- Token-based authentication (optional)

✅ **VLC Integration**
- Direct M3U playlist support
- One-click browsing in VLC
- Works on all platforms (Windows, Mac, Linux, iOS, Android)

## System Requirements

- Android 10+
- Termux app
- .NET Core 8 SDK
- At least 500MB free storage
- 1GB+ RAM recommended

## Installation on Android (Termux)

### Step 1: Install Termux
1. Download **Termux** from F-Droid (https://f-droid.org/packages/com.termux/)
2. Install and launch the app
3. Grant storage permissions when prompted

### Step 2: Install .NET Core 8

```bash
# Update package list
pkg update
pkg upgrade

# Install dependencies
pkg install clang cmake make openssl git wget

# Download and install .NET Core 8
wget https://aka.ms/dotnet/8.0.0/dotnet-sdk-8.0.0-linux-arm64.tar.gz
mkdir -p ~/dotnet
tar xf dotnet-sdk-8.0.0-linux-arm64.tar.gz -C ~/dotnet
rm dotnet-sdk-8.0.0-linux-arm64.tar.gz

# Add to PATH
echo 'export DOTNET_ROOT=~/dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT' >> ~/.bashrc
source ~/.bashrc

# Verify installation
dotnet --version
```

### Step 3: Clone and Run Music Server

```bash
# Clone repository
git clone https://github.com/Maximus2k04/music-server.git
cd music-server

# Restore dependencies
dotnet restore

# Run the server
dotnet run
```

The server will start on `http://localhost:5000`

## Configuration

Edit `appsettings.json` to customize:

```json
{
  "MusicPath": "/sdcard/Music",           // Path to your music files
  "BaseUrl": "http://192.168.1.100:5000", // Your phone's IP address
  "AuthToken": "your-secret-token",       // Optional authentication token
  "RequireAuth": false,                    // Require token for all requests
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"      // Server port
      }
    }
  }
}
```

### Music Paths on Android

- `/sdcard/Music` - Internal storage
- `/sdcard/DCIM` - Photos/Videos with embedded audio
- `/storage/emulated/0/Music` - Alternative internal storage path
- `/sdcard/Podcasts` - Podcast directory

## API Endpoints

### Playlists
- `GET /api/playlist/all` - Master playlist (all songs)
- `GET /api/playlist/artists` - Browse by artist
- `GET /api/playlist/artist/{name}` - Artist's songs
- `GET /api/playlist/albums` - Browse by album
- `GET /api/playlist/album/{name}` - Album songs
- `GET /api/playlist/genres` - Browse by genre
- `GET /api/playlist/genre/{name}` - Genre songs

### Streaming
- `GET /api/stream/{songId}` - Stream audio file
- Supports range requests for seeking

### Library
- `GET /api/library/songs` - All songs metadata
- `GET /api/library/artists` - All artists
- `GET /api/library/albums` - All albums
- `GET /api/library/genres` - All genres
- `GET /api/library/stats` - Library statistics
- `POST /api/library/refresh` - Rescan directory

### Search
- `GET /api/search?q={query}` - Search songs

### Health
- `GET /api/health` - Server status

## VLC Media Player Setup

### Local Network Access

1. **Get Your Phone's IP Address** (in Termux):
   ```bash
   ifconfig wlan0
   # Note the inet address (e.g., 192.168.1.100)
   ```

2. **Open VLC** on your desktop/phone
3. Go to **Media → Open Network Stream**
4. Enter: `http://192.168.1.100:5000/api/playlist/all`
5. Click **Play**
6. Browse and play your music!

### Global Access (Internet)

#### Option 1: Port Forwarding

1. **Configure Router**:
   - Open router admin panel (usually 192.168.1.1)
   - Find Port Forwarding section
   - Forward external port (e.g., 8080) → phone internal port (5000)
   - Note your public IP address

2. **Update appsettings.json**:
   ```json
   "BaseUrl": "http://your-public-ip:8080"
   ```

3. **In VLC** (from anywhere):
   - `http://your-public-ip:8080/api/playlist/all`

#### Option 2: Dynamic DNS (Recommended)

If your ISP assigns dynamic IP addresses:

1. **Sign up for Dynamic DNS**:
   - Use services like: No-IP, DynDNS, or FreeDNS
   - Create a domain (e.g., `mymusic.hopto.org`)

2. **Update DDNS on Phone**:
   ```bash
   # In Termux, set up a cron job to update DDNS
   # (Install dcron: pkg install dcron)
   ```

3. **Configure HTTPS** (optional but recommended):
   - Use Let's Encrypt with certbot
   - Configure in appsettings.json

4. **In VLC**:
   - `https://mymusic.hopto.org:8080/api/playlist/all`

## Security Considerations

⚠️ **Important**: When exposing to the internet:

1. **Use HTTPS** instead of HTTP
2. **Enable authentication**: Set `RequireAuth: true` and use a strong token
3. **Change default port** from 5000
4. **Monitor access logs** in Termux
5. **Use a firewall** on your router
6. **Keep Android updated** for security patches

## Troubleshooting

### "Music files not found"
- Check music path in appsettings.json
- Verify files are in supported formats (MP3, FLAC, WAV, OGG, M4A)
- Grant storage permissions to Termux

### "VLC can't connect"
- Verify server is running: `dotnet run`
- Check phone's IP address: `ifconfig wlan0`
- Ensure firewall allows port 5000
- Verify phone and VLC device are on same network

### "Slow streaming"
- Use 5GHz WiFi if available
- Reduce bitrate by re-encoding music files
- Close other bandwidth-consuming apps
- Increase buffer in VLC: Tools → Preferences → Input/Codecs → Increase caching

### "Server crashes after X songs"
- Increase RAM allocation
- Reduce refresh interval
- Clear Termux cache: `pkg clean`

## Building from Source

```bash
# Restore NuGet packages
dotnet restore

# Build release version
dotnet publish -c Release -o ./publish

# Run published version
dotnet ./publish/MusicServer.dll
```

## Docker (Optional)

If you have Docker installed in Termux:

```bash
docker build -t music-server .
docker run -p 5000:5000 -v /sdcard/Music:/app/music music-server
```

## Performance Tips

1. **Index large libraries** by genre/artist for faster browsing
2. **Transcode FLAC** to MP3 for lower bandwidth
3. **Run on phone charger** to prevent throttling
4. **Close background apps** before streaming
5. **Use 5GHz WiFi** for better speeds

## Limitations

- No native UI (control via VLC)
- One server instance per phone
- Limited by phone's processing power
- Battery drain when streaming continuously
- No user account management

## Future Enhancements

- [ ] Web UI for browsing
- [ ] Podcast support
- [ ] Lyrics display
- [ ] User accounts and playlists
- [ ] Transcoding on-the-fly
- [ ] Mobile app
- [ ] Bluetooth speaker integration

## License

MIT License - Feel free to use and modify

## Contributing

Contributions welcome! Please submit pull requests or open issues.

## Support

For issues or questions:
- Check troubleshooting section
- Review Swagger docs: `http://localhost:5000/swagger`
- Open GitHub issue

---

**Made with ❤️ for music lovers**
