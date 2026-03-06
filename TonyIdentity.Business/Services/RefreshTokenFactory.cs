using System.Security.Cryptography;
using System.Text;

namespace TonyIdentity.Business.Services;

public static class RefreshTokenFactory
{
    public static string GenerateOpaqueToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public static string ComputeHash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
