using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LC360.Models;

[Table("login_attempts")]
public class LoginAttempt : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("ip_address")]
    public string IpAddress { get; set; } = string.Empty;

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("device_hash")]
    public string? DeviceHash { get; set; }

    [Column("success")]
    public bool Success { get; set; } = false;

    [Column("country_code")]
    public string? CountryCode { get; set; }

    [Column("city")]
    public string? City { get; set; }

    [Column("attempt_time")]
    public DateTime AttemptTime { get; set; } = DateTime.UtcNow;
}