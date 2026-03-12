using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonyIdentity.Business.Interfaces;
using TonyIdentity.Business.Models.Auth;
using TonyIdentity.Business.Options;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshSessionService _refreshSessionService;
        private readonly ITwoFactorChallengeService _twoFactorChallengeService;
        private readonly IEmailSender _emailSender;
        private readonly JwtOptions _jwtOptions;
        private readonly FrontendOptions _frontendOptions;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            IRefreshSessionService refreshSessionService,
            ITwoFactorChallengeService twoFactorChallengeService,
            IEmailSender emailSender,
            IOptions<JwtOptions> jwtOptions,
            IOptions<FrontendOptions> frontendOptions,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _refreshSessionService = refreshSessionService;
            _twoFactorChallengeService = twoFactorChallengeService;
            _emailSender = emailSender;
            _jwtOptions = jwtOptions.Value;
            _frontendOptions = frontendOptions.Value;
            _logger = logger;

        }

        public async Task ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null) 
            {
                throw new InvalidOperationException("Invalid user.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) 
            {
                var errorMessage = string.Join("; ", result.Errors.Select(x => x.Description));
                throw new InvalidOperationException(errorMessage);
            }
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestModel model, CancellationToken cancellationToken)
        {
            var email = model.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) 
            {
                _logger.LogInformation("Forgot password requested for non-existing email {Email}", email);
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var url = $"{_frontendOptions.ResetPasswordUrl}?email={Uri.EscapeDataString(email)}&token={encodedToken}";

            await _emailSender.SendPaswordResetAsync(email, url);
        }

        public async Task<LoginResultModel> LoginAsync(LoginRequestModel model, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
        {
            var email = model?.Email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid credentials.");
            }

            if (user.IsBlocked)
            {
                throw new InvalidOperationException("User is blocked.");
            }

            if (!user.EmailConfirmed)
            {
                throw new InvalidOperationException("Email is not confirmed.");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);
            if (!signInResult.Succeeded) 
            {
                if (signInResult.IsLockedOut)
                {
                    throw new InvalidOperationException("User is locked out.");
                }

                throw new InvalidOperationException("Invalid credentials.");
            }

            if (user.TwoFactorEnabled) 
            {
                var challengeToken = await _twoFactorChallengeService.CreateChallengeAsync(
                    user,
                    ipAddress,
                    userAgent,
                    cancellationToken);

                return new LoginResultModel
                {
                    RequiresTwoFactor = true,
                    TwoFactorChallengeToken = challengeToken,
                };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, roles);
            var refreshToken = _jwtTokenService.GenerateRandomToken();

            await _refreshSessionService.createOrReplaceSessionAsync(
                user,
                refreshToken,
                ipAddress,
                userAgent,
                cancellationToken);

            return new LoginResultModel
            {
                RequiresTwoFactor = false,
                TokenResponse = new TokenResponseModel
                {
                    AccessToken = accessToken,
                    ExpiresInSeconds = _jwtOptions.AccessTokenMinutes * 60,
                    RefreshToken = refreshToken,
                }
            };
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
        {
            await _refreshSessionService.ReevokeSessionAsync(refreshToken, cancellationToken);
        }

        public async Task<TokenResponseModel> RefreshAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
        {
            var session = await _refreshSessionService.GetValidSessionByTokenAsync(refreshToken, cancellationToken);
            if (session == null || session.User == null) 
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            if (session.User.IsBlocked)
            {
                throw new InvalidOperationException("User is blocked.");
            }

            var newRefreshToken = _jwtTokenService.GenerateRandomToken();

            await _refreshSessionService.UpdateSessionTokenAsync(
                session,
                newRefreshToken,
                ipAddress,
                userAgent,
                cancellationToken);

            var roles = await _userManager.GetRolesAsync(session.User);
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(session.User, roles);

            return new TokenResponseModel
            {
                AccessToken = accessToken,
                ExpiresInSeconds = _jwtOptions.AccessTokenMinutes * 60,
                RefreshToken = refreshToken,
            };
        }

        public async Task RegisterAsync(RegisterRequestModel model, CancellationToken cancellationToken)
        {
            var email = model.Email.Trim().ToLowerInvariant();

            var existingUser = await _userManager.FindByNameAsync(email);

            if (existingUser != null) 
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                CreatedAtUtc = DateTime.UtcNow,
                IsBlocked = false,
                EmailConfirmed = false,
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded) 
            {
                var errorMessage = string.Join("; ", createResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException(errorMessage);
            }

            await _userManager.AddToRoleAsync(user, "User");

            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(emailToken);
            var url = $"{_frontendOptions.ConfirmEmailUrl}?userId={user.Id}&token={encodedToken}";

            await _emailSender.SendEmailConfirmationAsync(user.Email!, url);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestModel model, CancellationToken cancellationToken)
        {
            var email = model.Email.ToLower().ToLowerInvariant();
            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
            {
                throw new InvalidOperationException("Invalid password reset request.");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(x => x.Description));
                throw new InvalidOperationException(errorMessage);
            }
        }

        public async Task<TokenResponseModel> VerifyTwoFactorAsync(VerifyTwoFactorRequestModel model, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
        {
            var challenge = await _twoFactorChallengeService.GetValidChallengeAsync(model.TwoFactorChallengeToken, cancellationToken);

            if (challenge == null) 
            {
                throw new InvalidOperationException("Invalid or expired two-factor challenge.");
            }

            var user = challenge.User;

            if (user.IsBlocked)
            {
                throw new InvalidOperationException("User is blocked.");
            }

            var isValidCode = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                model.Code);

            if (!isValidCode)
            {
                throw new InvalidOperationException("Invalid two-factor code.");
            }

            await _twoFactorChallengeService.MarkUsedAsync(challenge, cancellationToken);

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, roles);
            var refreshToken = _jwtTokenService.GenerateRandomToken();

            await _refreshSessionService.createOrReplaceSessionAsync(
                user,
                refreshToken,
                ipAddress,
                userAgent,
                cancellationToken);

            return new TokenResponseModel
            {
                AccessToken = accessToken,
                ExpiresInSeconds = _jwtOptions.AccessTokenMinutes * 60,
                RefreshToken = refreshToken
            };
        }
    }
}
