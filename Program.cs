using MusicServer;
using MusicServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Music Server API",
        Version = "v1.0.0",
        Description = "A mobile-based music streaming server with UPnP/DLNA support for VLC Media Player",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Music Server"
        }
    });
});

// Add custom services
builder.Services.AddSingleton<MusicService>();
builder.Services.AddSingleton<PlaylistService>();
builder.Services.AddSingleton<StreamingService>();
builder.Services.AddSingleton<AuthService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Music Server API v1"));
}

app.UseRouting();
app.UseCors("AllowAll");
app.MapControllers();

// Initialize music service on startup
var musicService = app.Services.GetRequiredService<MusicService>();
await musicService.InitializeAsync();

Console.WriteLine("\n╔════════════════════════════════════════╗");
Console.WriteLine("║    🎵 Music Server Starting 🎵         ║");
Console.WriteLine($"║    Songs loaded: {musicService.GetTotalSongCount()}");
Console.WriteLine("║    API: http://localhost:5000          ║");
Console.WriteLine("║    Swagger: http://localhost:5000/swagger ║");
Console.WriteLine("╚════════════════════════════════════════╝\n");

app.Run();
