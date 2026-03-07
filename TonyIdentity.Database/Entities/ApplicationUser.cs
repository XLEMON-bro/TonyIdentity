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
        public DateTime? BlockedAt { get; set; }
        public string? BlockedReason { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? LastLoginAtUtc { get; set; }

        public ICollection<RefreshTokenEntity> RefreshTokens { get; set; }

        public ApplicationUser() 
        {
            RefreshTokens = new List<RefreshTokenEntity>();
        }
    }
}
