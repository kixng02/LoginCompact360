using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LC360.Models;

[Table("token_revocation_list")]
public class TokenRevocationEntry : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("token_jti")]
    public string TokenJti { get; set; } = string.Empty;

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("revoked_at")]
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("reason")]
    public string? Reason { get; set; } // PASSWORD_CHANGE | FORCED_LOGOUT | SUSPICIOUS_ACTIVITY
}