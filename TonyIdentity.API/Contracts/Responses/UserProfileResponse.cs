namespace TonyIdentity.API.Contracts.Responses;

public class UserProfileResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
}
