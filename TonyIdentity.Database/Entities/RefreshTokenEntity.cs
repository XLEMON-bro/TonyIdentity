namespace TonyIdentity.Database.Entities
{
    public class RefreshTokenEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string TokenHash { get; set; }
        public string TokenSalt { get;set; }

        public Guid SessionId { get; set; }
        public Guid? ReplacedByTokenId { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }
        public string? RevokedReason { get; set; }

        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
        public bool IsRevoked => RevokedAtUtc.HasValue;
        public bool IsActive => !IsExpired && !IsRevoked;
    }
}