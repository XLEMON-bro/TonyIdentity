namespace TonyIdentity.Business.Models;

public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public TokenResult? Tokens { get; set; }
}
