namespace MusicServer.Services;

public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly string? _validToken;
    private readonly bool _requireAuth;

    public AuthService(ILogger<AuthService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _validToken = configuration["AuthToken"];
        _requireAuth = configuration.GetValue<bool>("RequireAuth", false);
    }

    public bool ValidateToken(string? token)
    {
        if (!_requireAuth)
            return true;

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Request without authentication token");
            return false;
        }

        var isValid = token == _validToken;
        if (!isValid)
            _logger.LogWarning($"Invalid authentication token attempt");

        return isValid;
    }
}
