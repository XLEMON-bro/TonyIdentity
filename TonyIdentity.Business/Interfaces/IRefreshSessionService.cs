using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Business.Interfaces
{
    public interface IRefreshSessionService
    {
        Task<RefreshSessionEntity> createOrReplaceSessionAsync(
            ApplicationUser user,
            string refreshtoken,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken);

        Task<RefreshSessionEntity?> GetValidSessionByTokenAsync(string refreshToken, CancellationToken cancellation);

        Task UpdateSessionTokenAsync(
            RefreshSessionEntity session,
            string newRefreshToken,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken);

        Task ReevokeSessionAsync(string refreshToken, CancellationToken cancellationToken);
    }
}
