using System.ComponentModel.DataAnnotations;

namespace TonyIdentity.API.Contracts.Requests;

public class VerifyTotpRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;
}
