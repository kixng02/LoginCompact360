using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LC360.Services;
using LC360.Models;

namespace LC360.Components.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly Supabase.Client   _supabase;
    private readonly SupabaseService   _supabaseService;
    private readonly IConfiguration    _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        Supabase.Client supabase,
        SupabaseService supabaseService,
        IConfiguration config,
        ILogger<AuthController> logger)
    {
        _supabase        = supabase;
        _supabaseService = supabaseService;
        _config          = config;
        _logger          = logger;
    }

    // ── POST /api/auth/register ───────────────────────────────────────────
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var session = await _supabase.Auth.SignUp(request.Email, request.Password);

            if (session?.User == null)
                return BadRequest(new { message = "Registration failed. Please try again." });

            _logger.LogInformation("User registered: {Email}", request.Email);
            return Ok(new { message = "Registration successful. Please check your email to confirm your account." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error for {Email}", request.Email);
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── POST /api/auth/login ──────────────────────────────────────────────
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = GetClientIp();

        try
        {
            var session = await _supabase.Auth.SignIn(request.Email, request.Password);

            if (session?.User == null)
            {
                await LogAttemptAsync(ip, request.Email, success: false);
                return Unauthorized(new { message = "Invalid email or password." });
            }

            await LogAttemptAsync(ip, request.Email, success: true);

            var token   = GenerateJwt(session.User.Id!, request.Email);
            var expiry  = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

            return Ok(new
            {
                token,
                expiresIn = expiry * 60,   // seconds
                email     = request.Email,
                userId    = session.User.Id
            });
        }
        catch (Exception ex)
        {
            await LogAttemptAsync(ip, request.Email, success: false);
            _logger.LogWarning(ex, "Login failed for {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password." });
        }
    }

    // ── POST /api/auth/logout ─────────────────────────────────────────────
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _supabase.Auth.SignOut();
            return Ok(new { message = "Logged out successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout error");
            return StatusCode(500, new { message = "Logout failed." });
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private string GenerateJwt(string userId, string email)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jti   = Guid.NewGuid().ToString();
        var expiry = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti,   jti),
            new Claim(ClaimTypes.NameIdentifier,     userId),
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task LogAttemptAsync(string ip, string email, bool success)
    {
        await _supabaseService.LogLoginAttemptAsync(new LoginAttempt
        {
            IpAddress   = ip,
            Username    = email,
            Success     = success,
            AttemptTime = DateTime.UtcNow
        });
    }

    private string GetClientIp()
    {
        var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

// ── Request / Response DTOs ───────────────────────────────────────────────────

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public record LoginRequest(string Email, string Password);
