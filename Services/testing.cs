// Services/AuthService.cs
using LC360.Models;
using System.Collections.Concurrent;

namespace LC360.Services;

public class AuthService : IAuthService
{
    // In-memory store: email -> password (plain text until PostgreSQL + bcrypt are wired up)
    private static readonly ConcurrentDictionary<string, string> _users =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly ILogger<AuthService> _logger;

    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger;
    }

    public async Task RegisterAsync(SignupRequest request)
    {
        await Task.Delay(500);

        if (!_users.TryAdd(request.Email, request.Password))
            throw new Exception("An account with that email already exists.");

        _logger.LogInformation("User registered: {Email}", request.Email);
    }

    public Task<bool> LoginAsync(string email, string password)
    {
        if (_users.TryGetValue(email, out var stored) && stored == password)
            return Task.FromResult(true);

        return Task.FromResult(false);
    }

    public Task LogoutAsync() => Task.CompletedTask;

    public Task<bool> IsAuthenticatedAsync() => Task.FromResult(false);
}
