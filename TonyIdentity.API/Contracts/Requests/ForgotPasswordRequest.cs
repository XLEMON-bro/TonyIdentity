using System.ComponentModel.DataAnnotations;

namespace TonyIdentity.API.Contracts.Requests;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
