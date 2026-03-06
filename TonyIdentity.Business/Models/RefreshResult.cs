namespace TonyIdentity.Business.Models;

public class RefreshResult : AuthResult
{
    public bool RefreshTokenReused { get; set; }
}
