using System.ComponentModel.DataAnnotations;

namespace TonyIdentity.API.Contracts.Requests;

public class ResetPasswordRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}
