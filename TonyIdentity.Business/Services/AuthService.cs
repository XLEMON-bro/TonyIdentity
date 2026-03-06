using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TonyIdentity.Business.Abstractions;
using TonyIdentity.Business.Models;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Business.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IEmailSender emailSender,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<OperationResult> RegisterAsync(RegisterUserRequest request, string origin, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "register_failed",
                ErrorMessage = string.Join("; ", createResult.Errors.Select(x => x.Description))
            };
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encoded = HttpUtility.UrlEncode(token);
        var link = $"{origin.TrimEnd('/')}/api/auth/confirm-email?userId={user.Id}&token={encoded}";

        await _emailSender.SendEmailConfirmationAsync(user.Email!, link, cancellationToken);
        return new OperationResult { Success = true };
    }

    public async Task<OperationResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return new OperationResult { Success = false, ErrorCode = "invalid_request", ErrorMessage = "Invalid confirmation request." };
        }

        var decodedToken = HttpUtility.UrlDecode(token);
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken!);

        return result.Succeeded
            ? new OperationResult { Success = true }
            : new OperationResult { Success = false, ErrorCode = "confirm_failed", ErrorMessage = "Email confirmation failed." };
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new AuthResult { Success = false, ErrorCode = "invalid_credentials", ErrorMessage = "Invalid credentials." };
        }

        if (user.IsBlocked)
        {
            return new AuthResult { Success = false, ErrorCode = "blocked", ErrorMessage = "User is blocked." };
        }

        if (!user.EmailConfirmed)
        {
            return new AuthResult { Success = false, ErrorCode = "email_not_confirmed", ErrorMessage = "Email must be confirmed before login." };
        }

        var passwordResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (passwordResult.IsLockedOut)
        {
            return new AuthResult { Success = false, ErrorCode = "locked_out", ErrorMessage = "User is locked out due to failed attempts." };
        }

        if (!passwordResult.Succeeded)
        {
            return new AuthResult { Success = false, ErrorCode = "invalid_credentials", ErrorMessage = "Invalid credentials." };
        }

        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            if (!string.IsNullOrWhiteSpace(request.TotpCode))
            {
                var validTotp = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, request.TotpCode);
                if (!validTotp)
                {
                    return new AuthResult { Success = false, ErrorCode = "invalid_mfa", ErrorMessage = "Invalid MFA code." };
                }
            }
            else if (!string.IsNullOrWhiteSpace(request.RecoveryCode))
            {
                var recoveryResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, request.RecoveryCode);
                if (!recoveryResult.Succeeded)
                {
                    return new AuthResult { Success = false, ErrorCode = "invalid_mfa", ErrorMessage = "Invalid recovery code." };
                }
            }
            else
            {
                return new AuthResult { Success = false, ErrorCode = "mfa_required", ErrorMessage = "MFA code is required." };
            }
        }

        var tokens = await _tokenService.IssueTokensAsync(user, ipAddress, cancellationToken);
        return new AuthResult { Success = true, Tokens = tokens };
    }

    public async Task<OperationResult> ForgotPasswordAsync(string email, string origin, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.EmailConfirmed)
        {
            // anti-enumeration: behave identically for unknown users
            return new OperationResult { Success = true };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encoded = HttpUtility.UrlEncode(token);
        var link = $"{origin.TrimEnd('/')}/api/auth/reset-password?userId={user.Id}&token={encoded}";

        await _emailSender.SendResetPasswordAsync(user.Email!, link, cancellationToken);
        return new OperationResult { Success = true };
    }

    public async Task<OperationResult> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return new OperationResult { Success = false, ErrorCode = "invalid_request", ErrorMessage = "Invalid reset request." };
        }

        var decodedToken = HttpUtility.UrlDecode(token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken!, newPassword);
        if (!result.Succeeded)
        {
            return new OperationResult { Success = false, ErrorCode = "reset_failed", ErrorMessage = "Password reset failed." };
        }

        await _userManager.UpdateSecurityStampAsync(user);
        return new OperationResult { Success = true };
    }

    public async Task<OperationResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return new OperationResult { Success = false, ErrorCode = "user_not_found", ErrorMessage = "User not found." };
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            return new OperationResult { Success = false, ErrorCode = "change_failed", ErrorMessage = "Unable to change password." };
        }

        await _userManager.UpdateSecurityStampAsync(user);
        _logger.LogInformation("Password changed for user {UserId}", userId);
        return new OperationResult { Success = true };
    }
}
