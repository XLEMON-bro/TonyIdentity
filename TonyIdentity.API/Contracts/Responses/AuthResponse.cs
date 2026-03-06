namespace TonyIdentity.API.Contracts.Responses;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset AccessTokenExpiresAtUtc { get; set; }
}
