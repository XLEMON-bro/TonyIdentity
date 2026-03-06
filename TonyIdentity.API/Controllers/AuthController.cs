using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TonyIdentity.API.Contracts.Requests;
using TonyIdentity.API.Contracts.Responses;
using TonyIdentity.API.Extensions;
using TonyIdentity.Business.Abstractions;
using TonyIdentity.Business.Models;

namespace TonyIdentity.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshCookieName = "tid_refresh";
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [EnableRateLimiting("register")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(new RegisterUserRequest
        {
            Email = request.Email,
            Password = request.Password
        }, GetOrigin(), cancellationToken);

        if (!result.Success)
        {
            return Problem(title: result.ErrorCode, detail: result.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(new ApiMessageResponse { Message = "Registration successful. Please confirm your email." });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token, CancellationToken cancellationToken)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token, cancellationToken);
        if (!result.Success)
        {
            return Problem(title: result.ErrorCode, detail: result.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(new ApiMessageResponse { Message = "Email confirmed successfully." });
    }

    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Contracts.Requests.LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(new TonyIdentity.Business.Models.LoginRequest
        {
            Email = request.Email,
            Password = request.Password,
            TotpCode = request.TotpCode,
            RecoveryCode = request.RecoveryCode
        }, HttpContext.GetClientIp(), cancellationToken);

        if (!result.Success || result.Tokens is null)
        {
            return Problem(title: result.ErrorCode, detail: result.ErrorMessage, statusCode: StatusCodes.Status401Unauthorized);
        }

        SetRefreshCookie(result.Tokens.RefreshToken, result.Tokens.RefreshTokenExpiresAtUtc);

        return Ok(new AuthResponse
        {
            AccessToken = result.Tokens.AccessToken,
            AccessTokenExpiresAtUtc = result.Tokens.AccessTokenExpiresAtUtc
        });
    }

    [EnableRateLimiting("refresh")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var token = request.RefreshToken ?? Request.Cookies[RefreshCookieName];
        if (string.IsNullOrWhiteSpace(token))
        {
            return Problem(title: "invalid_refresh", detail: "Missing refresh token.", statusCode: StatusCodes.Status401Unauthorized);
        }

        var refreshResult = await _tokenService.RefreshAsync(token, HttpContext.GetClientIp(), cancellationToken);
        if (!refreshResult.Success || refreshResult.Tokens is null)
        {
            return Problem(title: refreshResult.ErrorCode, detail: refreshResult.ErrorMessage, statusCode: StatusCodes.Status401Unauthorized);
        }

        SetRefreshCookie(refreshResult.Tokens.RefreshToken, refreshResult.Tokens.RefreshTokenExpiresAtUtc);

        return Ok(new AuthResponse
        {
            AccessToken = refreshResult.Tokens.AccessToken,
            AccessTokenExpiresAtUtc = refreshResult.Tokens.AccessTokenExpiresAtUtc
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken, HttpContext.GetClientIp(), cancellationToken);
        }

        Response.Cookies.Delete(RefreshCookieName);
        return Ok(new ApiMessageResponse { Message = "Logged out." });
    }

    [EnableRateLimiting("forgot")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(request.Email, GetOrigin(), cancellationToken);

        return Ok(new ApiMessageResponse { Message = "If an account exists, instructions were sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request.UserId, request.Token, request.NewPassword, cancellationToken);

        if (!result.Success)
        {
            return Problem(title: result.ErrorCode, detail: result.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(new ApiMessageResponse { Message = "Password reset successful." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken);

        if (!result.Success)
        {
            return Problem(title: result.ErrorCode, detail: result.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(new ApiMessageResponse { Message = "Password changed successfully." });
    }

    private string GetOrigin()
    {
        return Request.Headers.Origin.FirstOrDefault() ?? $"{Request.Scheme}://{Request.Host}";
    }

    private void SetRefreshCookie(string refreshToken, DateTimeOffset expiresAt)
    {
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAt,
            IsEssential = true,
            Path = "/"
        });
    }
}
