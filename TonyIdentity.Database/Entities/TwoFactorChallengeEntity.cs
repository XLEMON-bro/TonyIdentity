using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonyIdentity.Database.Entities
{
    public class TwoFactorChallengeEntity
    {
        public Guid id { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string ChallengeTokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? CunsumedAtUtc { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
