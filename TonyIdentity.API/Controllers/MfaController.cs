using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TonyIdentity.API.Contracts.Requests;
using TonyIdentity.API.Contracts.Responses;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mfa")]
public class MfaController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public MfaController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("setup")]
    public async Task<IActionResult> GetSetupInfo()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized();
        }

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        return Ok(new { sharedKey = key, email = user.Email });
    }

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] VerifyTotpRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized();
        }

        var valid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);
        if (!valid)
        {
            return Problem(title: "invalid_mfa", detail: "Invalid authenticator code.", statusCode: StatusCodes.Status400BadRequest);
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return Ok(new { message = "MFA enabled.", recoveryCodes });
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized();
        }

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        return Ok(new ApiMessageResponse { Message = "MFA disabled." });
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return await _userManager.FindByIdAsync(userId.ToString());
    }
}
