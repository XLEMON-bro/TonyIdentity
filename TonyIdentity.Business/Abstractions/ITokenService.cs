using TonyIdentity.Business.Models;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Business.Abstractions;

public interface ITokenService
{
    Task<TokenResult> IssueTokensAsync(ApplicationUser user, string ipAddress, CancellationToken cancellationToken = default);
    Task<RefreshResult> RefreshAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
}
