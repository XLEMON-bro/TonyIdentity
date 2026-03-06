using Microsoft.IdentityModel.Tokens;
using TonyIdentity.Business.Models;

namespace TonyIdentity.Business.Abstractions;

public interface ISigningKeyProvider
{
    Task<SigningCredentialsModel> GetCurrentSigningCredentialsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<JwkModel>> GetPublicJwksAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SecurityKey>> GetValidationKeysAsync(CancellationToken cancellationToken = default);
}
