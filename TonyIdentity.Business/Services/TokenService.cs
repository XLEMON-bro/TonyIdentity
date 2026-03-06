using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TonyIdentity.Business.Abstractions;
using TonyIdentity.Business.Models;
using TonyIdentity.Business.Options;
using TonyIdentity.Database.Contexts;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Business.Services;

public class TokenService : ITokenService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISigningKeyProvider _signingKeyProvider;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ISigningKeyProvider signingKeyProvider,
        IOptions<JwtOptions> jwtOptions,
        ILogger<TokenService> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _signingKeyProvider = signingKeyProvider;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<TokenResult> IssueTokensAsync(ApplicationUser user, string ipAddress, CancellationToken cancellationToken = default)
    {
        var accessExpiry = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes);
        var signingData = await _signingKeyProvider.GetCurrentSigningCredentialsAsync(cancellationToken);
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("security_stamp", user.SecurityStamp ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwt = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: accessExpiry.UtcDateTime,
            signingCredentials: signingData.Credentials);

        jwt.Header["kid"] = signingData.Kid;

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refreshRaw = RefreshTokenFactory.GenerateOpaqueToken();
        var refreshHash = RefreshTokenFactory.ComputeHash(refreshRaw);
        var refreshExpiry = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetimeDays);

        var refreshEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAtUtc = refreshExpiry,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedByIp = ipAddress
        };

        _dbContext.RefreshTokens.Add(refreshEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TokenResult
        {
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessExpiry,
            RefreshToken = refreshRaw,
            RefreshTokenExpiresAtUtc = refreshExpiry
        };
    }

    public async Task<RefreshResult> RefreshAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var tokenHash = RefreshTokenFactory.ComputeHash(refreshToken);
        var currentToken = await _dbContext.RefreshTokens.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (currentToken is null)
        {
            return new RefreshResult { Success = false, ErrorCode = "invalid_refresh", ErrorMessage = "Invalid refresh token." };
        }

        if (currentToken.User.IsBlocked)
        {
            return new RefreshResult { Success = false, ErrorCode = "blocked", ErrorMessage = "User is blocked." };
        }

        if (currentToken.RevokedAtUtc is not null)
        {
            currentToken.IsCompromised = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Refresh token reuse detected for user {UserId}", currentToken.UserId);
            return new RefreshResult { Success = false, RefreshTokenReused = true, ErrorCode = "reuse_detected", ErrorMessage = "Refresh token has been reused." };
        }

        if (currentToken.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return new RefreshResult { Success = false, ErrorCode = "expired_refresh", ErrorMessage = "Refresh token expired." };
        }

        var newTokens = await IssueTokensAsync(currentToken.User, ipAddress, cancellationToken);

        var newHash = RefreshTokenFactory.ComputeHash(newTokens.RefreshToken);
        var replacement = await _dbContext.RefreshTokens.FirstAsync(x => x.TokenHash == newHash, cancellationToken);

        currentToken.RevokedAtUtc = DateTimeOffset.UtcNow;
        currentToken.RevokedByIp = ipAddress;
        currentToken.ReplacedByTokenId = replacement.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshResult { Success = true, Tokens = newTokens };
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var tokenHash = RefreshTokenFactory.ComputeHash(refreshToken);
        var tokenEntity = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (tokenEntity is null || tokenEntity.RevokedAtUtc is not null)
        {
            return;
        }

        tokenEntity.RevokedAtUtc = DateTimeOffset.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
