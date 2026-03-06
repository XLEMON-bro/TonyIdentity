using Microsoft.AspNetCore.Identity;

namespace TonyIdentity.Database.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsBlocked { get; set; }
    public DateTimeOffset? BlockedAt { get; set; }
    public string? BlockedReason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
