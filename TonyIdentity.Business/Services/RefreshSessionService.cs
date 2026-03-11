

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TonyIdentity.Business.Interfaces;
using TonyIdentity.Business.Options;
using TonyIdentity.Database.Context;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Business.Services
{
    public class RefreshSessionService : IRefreshSessionService
    {
        private readonly TonyIdentityDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly RefreshTokenOptions _refreshTokenOptions;

        public RefreshSessionService(
            TonyIdentityDbContext dbContext,
            IJwtTokenService jwtTokenService,
            IOptions<RefreshTokenOptions> refreshTokenOptions)
        {
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
            _refreshTokenOptions = refreshTokenOptions.Value;
        }

        public async Task<RefreshSessionEntity> createOrReplaceSessionAsync(ApplicationUser user, string refreshtoken, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
        {
            var tokenHash = _jwtTokenService.ComputeHash(refreshtoken);

            var existingActiveSession = await _dbContext.RefreshSessions
                .Where(x => x.UserId == user.Id && x.RevokedAtUtc == null)
                .OrderByDescending(x => x.UpdatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            var now = DateTime.UtcNow;

            if (existingActiveSession == null) 
            {
                existingActiveSession = new RefreshSessionEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TokenHash = tokenHash,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                    ExpiresAtUtc = now.AddDays(_refreshTokenOptions.RefreshTokenDays),
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                };
            }
            else
            {
                existingActiveSession.TokenHash = tokenHash;
                existingActiveSession.UpdatedAtUtc = now;
                existingActiveSession.ExpiresAtUtc = now.AddDays(_refreshTokenOptions.RefreshTokenDays);
                existingActiveSession.IpAddress = ipAddress;
                existingActiveSession.UserAgent = userAgent;
                existingActiveSession.RevokedAtUtc = null;
            }

            await _dbContext.SaveChangesAsync();

            return existingActiveSession;
        }

        public async Task<RefreshSessionEntity?> GetValidSessionByTokenAsync(string refreshToken, CancellationToken cancellation)
        {
            var tokenHash = _jwtTokenService.ComputeHash(refreshToken);

            return await _dbContext.RefreshSessions.Include(x => x.User)
                .FirstOrDefaultAsync(x => 
                    x.TokenHash == tokenHash && 
                    x.RevokedAtUtc == null && 
                    x.ExpiresAtUtc > DateTime.UtcNow,
                    cancellation);
        }

        public async Task ReevokeSessionAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var tokenHash = _jwtTokenService.ComputeHash(refreshToken);

            var session = await _dbContext.RefreshSessions
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

            if (session == null) 
            {
                return;
            }

            var now = DateTime.UtcNow;

            session.RevokedAtUtc = now;
            session.UpdatedAtUtc = now;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateSessionTokenAsync(RefreshSessionEntity session, string newRefreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            session.TokenHash = _jwtTokenService.ComputeHash(newRefreshToken);
            session.UpdatedAtUtc = now;
            session.ExpiresAtUtc = now.AddDays(_refreshTokenOptions.RefreshTokenDays);
            session.IpAddress = ipAddress;
            session.UserAgent = userAgent;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
