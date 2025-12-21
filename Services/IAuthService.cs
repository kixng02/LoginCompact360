// Services/IAuthService.cs
using LC360.Models;

namespace LC360.Services;

public interface IAuthService
{
    Task RegisterAsync(SignupRequest request);
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
}