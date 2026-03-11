namespace TonyIdentity.Database.Entities
{
    public class RefreshSessionEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }

        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public string? DeviceName { get; set; }
    }
}