using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LC360.Models;

[Table("archive_log")]
public class ArchiveLog : BaseModel
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
    public bool Success { get; set; }

    [Column("country_code")]
    public string? CountryCode { get; set; }

    [Column("attempt_time")]
    public DateTime AttemptTime { get; set; }

    [Column("archived_at")]
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
}