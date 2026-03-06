namespace TonyIdentity.Business.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TotpCode { get; set; }
    public string? RecoveryCode { get; set; }
}
