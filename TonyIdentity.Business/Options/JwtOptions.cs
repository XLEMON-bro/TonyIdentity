namespace TonyIdentity.Business.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "https://identity.tonysecurity.com";
    public string Audience { get; set; } = "tonyfood-api";
    public int AccessTokenLifetimeMinutes { get; set; } = 5;
    public int RefreshTokenLifetimeDays { get; set; } = 2;
}
