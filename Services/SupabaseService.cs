using LC360.Models;
using Supabase;
using System.Text.Json;

namespace LC360.Services;

public class SupabaseService
{
    private readonly Client _supabase;
    private readonly ILogger<SupabaseService> _logger;

    public SupabaseService(Client supabase, ILogger<SupabaseService> logger)
    {
        _supabase = supabase;
        _logger = logger;
    }

    // ── Login Attempts ────────────────────────────────────────────────────

    public async Task LogLoginAttemptAsync(LoginAttempt attempt)
    {
        try
        {
            await _supabase.From<LoginAttempt>().Insert(attempt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log login attempt for {Username}", attempt.Username);
        }
    }

    public async Task<List<LoginAttempt>> GetRecentAttemptsAsync(int minutes = 60)
    {
        try
        {
            var since = DateTime.UtcNow.AddMinutes(-minutes);
            var result = await _supabase
                .From<LoginAttempt>()
                .Filter("attempt_time", Supabase.Postgrest.Constants.Operator.GreaterThan, since.ToString("o"))
                .Filter("success", Supabase.Postgrest.Constants.Operator.Equals, "false")
                .Get();
            return result.Models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch recent login attempts");
            return new List<LoginAttempt>();
        }
    }

    // ── Security Alerts ───────────────────────────────────────────────────

    public async Task CreateAlertAsync(string alertType, int riskScore,
        string? ipAddress = null, string? username = null, object? details = null)
    {
        try
        {
            var alert = new SecurityAlert
            {
                AlertType = alertType,
                RiskScore = riskScore,
                IpAddress = ipAddress,
                Username  = username,
                Details   = details != null ? JsonSerializer.Serialize(details) : null,
                Resolved  = false,
                CreatedAt = DateTime.UtcNow
            };
            await _supabase.From<SecurityAlert>().Insert(alert);
            _logger.LogWarning("Security alert created: {Type} | Score: {Score}", alertType, riskScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create security alert");
        }
    }

    public async Task<List<SecurityAlert>> GetUnresolvedAlertsAsync()
    {
        try
        {
            var result = await _supabase
                .From<SecurityAlert>()
                .Filter("resolved", Supabase.Postgrest.Constants.Operator.Equals, "false")
                .Order("risk_score", Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();
            return result.Models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch unresolved alerts");
            return new List<SecurityAlert>();
        }
    }

    // ── Token Revocation List ─────────────────────────────────────────────

    public async Task RevokeTokenAsync(string jti, Guid userId, DateTime expiresAt, string reason)
    {
        try
        {
            var entry = new TokenRevocationEntry
            {
                TokenJti  = jti,
                UserId    = userId,
                ExpiresAt = expiresAt,
                Reason    = reason,
                RevokedAt = DateTime.UtcNow
            };
            await _supabase.From<TokenRevocationEntry>().Insert(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke token {Jti}", jti);
        }
    }

    public async Task<bool> IsTokenRevokedAsync(string jti)
    {
        try
        {
            var result = await _supabase
                .From<TokenRevocationEntry>()
                .Filter("token_jti", Supabase.Postgrest.Constants.Operator.Equals, jti)
                .Get();
            return result.Models.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check token revocation for {Jti}", jti);
            return false;
        }
    }

    // ── Archiving ─────────────────────────────────────────────────────────

    public async Task ArchiveOldAttemptsAsync(int daysOld = 90)
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysOld);
            var result = await _supabase
                .From<LoginAttempt>()
                .Filter("attempt_time", Supabase.Postgrest.Constants.Operator.LessThan, cutoff.ToString("o"))
                .Get();

            if (result.Models.Count == 0) return;

            var archives = result.Models.Select(a => new ArchiveLog
            {
                IpAddress   = a.IpAddress,
                Username    = a.Username,
                DeviceHash  = a.DeviceHash,
                Success     = a.Success,
                CountryCode = a.CountryCode,
                AttemptTime = a.AttemptTime,
                ArchivedAt  = DateTime.UtcNow
            }).ToList();

            await _supabase.From<ArchiveLog>().Insert(archives);
            _logger.LogInformation("Archived {Count} old login attempts", archives.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive old login attempts");
        }
    }
}