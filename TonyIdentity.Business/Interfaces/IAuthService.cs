using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonyIdentity.Business.Models.Auth;

namespace TonyIdentity.Business.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequestModel model, CancellationToken cancellationToken);
        Task ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken);
        Task<LoginResultModel> LoginAsync(LoginRequestModel model, string? ipAddress, string? userAgent, CancellationToken cancellationToken);
        Task<TokenResponseModel> VerifyTwoFactorAsync(VerifyTwoFactorRequestModel model, string? ipAddress, string? userAgent, CancellationToken cancellationToken);
        Task<TokenResponseModel> RefreshAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken);
        Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
        Task ForgotPasswordAsync(ForgotPasswordRequestModel model, CancellationToken cancellationToken);
        Task ResetPasswordAsync(ResetPasswordRequestModel model, CancellationToken cancellationToken);
    }
}
