using System;
using System.Text.Json.Nodes;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LC360.Models;

[Table("security_alerts")]
public class SecurityAlert : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("alert_type")]
    public string AlertType { get; set; } = string.Empty; // IP_FINGERPRINT | ACCOUNT_FOCUS

    [Column("risk_score")]
    public int RiskScore { get; set; }

    [Column("ip_address")]
    public string? IpAddress { get; set; }

    [Column("username")]
    public string? Username { get; set; }

    [Column("details")]
    public string? Details { get; set; } // JSON string

    [Column("resolved")]
    public bool Resolved { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}