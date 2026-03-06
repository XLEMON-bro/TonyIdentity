using System.ComponentModel.DataAnnotations;

namespace TonyIdentity.API.Contracts.Requests;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? TotpCode { get; set; }
    public string? RecoveryCode { get; set; }
}
