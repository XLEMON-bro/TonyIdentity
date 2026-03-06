using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TonyIdentity.Business.Abstractions;
using TonyIdentity.Business.Models;

namespace TonyIdentity.Business.Services;

public class LocalDevelopmentSigningKeyProvider : ISigningKeyProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocalDevelopmentSigningKeyProvider> _logger;
    private RSA? _cachedRsa;

    public LocalDevelopmentSigningKeyProvider(IConfiguration configuration, ILogger<LocalDevelopmentSigningKeyProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<SigningCredentialsModel> GetCurrentSigningCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var rsa = BuildRsa();
        var securityKey = new RsaSecurityKey(rsa)
        {
            KeyId = GetKid()
        };

        return Task.FromResult(new SigningCredentialsModel
        {
            Kid = securityKey.KeyId!,
            Credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256)
        });
    }

    public Task<IReadOnlyCollection<JwkModel>> GetPublicJwksAsync(CancellationToken cancellationToken = default)
    {
        var rsa = BuildRsa();
        var parameters = rsa.ExportParameters(false);

        var jwk = new JwkModel
        {
            Kid = GetKid(),
            N = Base64UrlEncoder.Encode(parameters.Modulus),
            E = Base64UrlEncoder.Encode(parameters.Exponent)
        };

        return Task.FromResult<IReadOnlyCollection<JwkModel>>(new List<JwkModel> { jwk });
    }


    public Task<IReadOnlyCollection<SecurityKey>> GetValidationKeysAsync(CancellationToken cancellationToken = default)
    {
        var key = new RsaSecurityKey(BuildRsa())
        {
            KeyId = GetKid()
        };

        return Task.FromResult<IReadOnlyCollection<SecurityKey>>(new List<SecurityKey> { key });
    }
    private RSA BuildRsa()
    {
        if (_cachedRsa is not null)
        {
            return _cachedRsa;
        }

        // For production, replace this with an Azure Key Vault based implementation.
        var privatePem = _configuration["Jwt:PrivateKeyPem"];

        if (string.IsNullOrWhiteSpace(privatePem))
        {
            _logger.LogWarning("Jwt private key was not provided. Generating an ephemeral development RSA key.");
            _cachedRsa = RSA.Create(2048);
            return _cachedRsa;
        }

        var rsa = RSA.Create();
        rsa.ImportFromPem(privatePem);
        _cachedRsa = rsa;
        return _cachedRsa;
    }

    private string GetKid()
    {
        return _configuration["Jwt:Kid"] ?? "local-dev-key-1";
    }
}
