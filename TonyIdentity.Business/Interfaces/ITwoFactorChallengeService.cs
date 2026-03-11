using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Business.Interfaces
{
    public interface ITwoFactorChallengeService
    {
        Task<string> CreateChallengeAsync(
            ApplicationUser user,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken);

        Task<TwoFactorChallengeEntity?> GetValidChallengeAsync(
            string challengeToken,
            CancellationToken cancellationToken);

        Task MarkUsedAsync(TwoFactorChallengeEntity challenge, CancellationToken cancellationToken);
    }
}
