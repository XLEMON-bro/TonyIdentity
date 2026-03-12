using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using TonyIdentity.Business.Interfaces;
using TonyIdentity.Business.Models.Auth;
using TonyIdentity.Business.Options;

namespace TonyIdentity.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth-policy")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly RefreshTokenOptions _refreshTokenOptions;
        private readonly IWebHostEnvironment _environment;

        public AuthController(
            IAuthService authService,
            IOptions<RefreshTokenOptions> refreshTokenOptions,
            IWebHostEnvironment environment)
        {
            _authService = authService;
            _refreshTokenOptions = refreshTokenOptions.Value;
            _environment = environment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model, CancellationToken cancellationToken)
        {
            await _authService.RegisterAsync(model, cancellationToken);

            return Ok(new
            {
                Message = "Registration successful. Please confirm your email."
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfrimEmail([FromQuery] Guid userId, [FromQuery] string token, CancellationToken cancellationToken)
        {
            await _authService.ConfirmEmailAsync(userId, token, cancellationToken);

            return Ok(new 
            {
                Message = "Email confirmed successfully."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model, CancellationToken cancellationToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();

            var result = await _authService.LoginAsync(model, ipAddress, userAgent, cancellationToken);
            if (result.RequiresTwoFactor)
            {
                return Ok(new
                {
                    RequiresTwoFactor = true,
                    TwoFactorChallengeToken = result.TwoFactorChallengeToken
                });
            }

            if (result.TokenResponse == null)
            {
                return BadRequest(new {Message = "Invalid login result."});
            }

            SetRefreshCookie(result.TokenResponse.RefreshToken);

            return Ok(new
            {
                RequiresTwoFactor = false,
                AccessToken = result.TokenResponse.AccessToken,
                ExpiresInSeconds = result.TokenResponse.ExpiresInSeconds
            });
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequestModel model, CancellationToken cancellationToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();

            var tokenResponse = await _authService.VerifyTwoFactorAsync(model, ipAddress, userAgent, cancellationToken);

            SetRefreshCookie(tokenResponse.RefreshToken);

            return Ok(new {
                AccessToken = tokenResponse.AccessToken,
                ExpiresInSeconds = tokenResponse.ExpiresInSeconds,
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            if (!Request.Cookies.TryGetValue(_refreshTokenOptions.CookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new { Message = "Refresh token cookie is missing." });
            }

            var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();

            var tokenResponse = await _authService.RefreshAsync(refreshToken, ipAddress, userAgent, cancellationToken);

            SetRefreshCookie(tokenResponse.RefreshToken);

            return Ok(new
            {
                AccessToken = tokenResponse.AccessToken,
                ExpiresInSeconds = tokenResponse.ExpiresInSeconds,
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            if(Request.Cookies.TryGetValue(_refreshTokenOptions.CookieName, out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken)){
                await _authService.LogoutAsync(refreshToken, cancellationToken);
            }

            Response.Cookies.Delete(_refreshTokenOptions.CookieName);

            return Ok(new { Message = "Logged out successfully."});
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel model, CancellationToken cancellationToken)
        {
            await _authService.ForgotPasswordAsync(model, cancellationToken);

            return Ok(new
            {
                Message = "If the email exists, a password reset link has been sent."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model, CancellationToken cancellationToken)
        {
            await _authService.ResetPasswordAsync(model, cancellationToken);

            return Ok(new { Message = "Password reset successful." });
        }

        private void SetRefreshCookie(string refreshToken) 
        {
            Response.Cookies.Append(_refreshTokenOptions.CookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment() ? true : false,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(_refreshTokenOptions.RefreshTokenDays),
                Path = "/"
            });
        }
    }
}
