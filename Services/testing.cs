// Services/AuthService.cs
using LC360.Models;
using System.Net.Http.Json;

namespace LC360.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;

    public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task RegisterAsync(SignupRequest request)
    {
        try
        {
            // Temporary: Simulate API call delay
            await Task.Delay(1000);
            
            // For now, just log and accept all registrations
            _logger.LogInformation("User registered: {Email}", request.Email);
            
            // In a real app, you would make an API call:
            // var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            // response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", request.Email);
            throw new Exception("Registration failed. Please try again.");
        }
    }

    public Task<bool> LoginAsync(string email, string password)
    {
        // Implement later
        return Task.FromResult(true);
    }

    public Task LogoutAsync()
    {
        // Implement later
        return Task.CompletedTask;
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        // Implement later
        return Task.FromResult(false);
    }
}