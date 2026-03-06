namespace TonyIdentity.Database.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? RevokedByIp { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }
    public Guid? ReplacedTokenId { get; set; }
    public RefreshToken? ReplacedToken { get; set; }
    public bool IsCompromised { get; set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTimeOffset.UtcNow && !IsCompromised;
}
