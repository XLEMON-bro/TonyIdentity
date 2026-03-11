using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonyIdentity.Database.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public bool IsBlocked { get;set; }
        public DateTime? BlockedAtUtc { get; set; }
        public string? BlockedReason { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public ICollection<RefreshSessionEntity> RefreshSessions { get; set; } = new List<RefreshSessionEntity>();
        public ICollection<TwoFactorChallengeEntity> TwoFactorChallenges { get; set; } = new List<TwoFactorChallengeEntity>();

    }
}
