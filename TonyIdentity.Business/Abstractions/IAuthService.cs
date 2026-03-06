using TonyIdentity.Business.Models;

namespace TonyIdentity.Business.Abstractions;

public interface IAuthService
{
    Task<OperationResult> RegisterAsync(RegisterUserRequest request, string origin, CancellationToken cancellationToken = default);
    Task<OperationResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<OperationResult> ForgotPasswordAsync(string email, string origin, CancellationToken cancellationToken = default);
    Task<OperationResult> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken cancellationToken = default);
    Task<OperationResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
