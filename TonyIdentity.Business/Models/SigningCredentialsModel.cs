using Microsoft.IdentityModel.Tokens;

namespace TonyIdentity.Business.Models;

public class SigningCredentialsModel
{
    public SigningCredentials Credentials { get; set; } = null!;
    public string Kid { get; set; } = string.Empty;
}
