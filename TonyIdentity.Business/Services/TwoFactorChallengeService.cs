using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonyIdentity.Business.Interfaces;
using TonyIdentity.Database.Context;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Business.Services
{
    public class TwoFactorChallengeService : ITwoFactorChallengeService
    {
        private readonly TonyIdentityDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;

        public TwoFactorChallengeService(TonyIdentityDbContext context, IJwtTokenService jwtTokenService)
        {
            _dbContext = context;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<string> CreateChallengeAsync(ApplicationUser user, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
        {
            var rawToken = _jwtTokenService.GenerateRandomToken();
            var hash = _jwtTokenService.ComputeHash(rawToken);

            var now = DateTime.UtcNow;

            var entity = new TwoFactorChallengeEntity
            {
                id = Guid.NewGuid(),
                UserId = user.Id,
                ChallengeHash = hash,
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(10),
                IsUsed = false,
                IpAddress = ipAddress,
                UserAgent = userAgent,
            };

            _dbContext.TwoFactorChallenges.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return rawToken;
        }

        public async Task<TwoFactorChallengeEntity?> GetValidChallengeAsync(string challengeToken, CancellationToken cancellationToken)
        {
            var hash = _jwtTokenService.ComputeHash(challengeToken);

            return await _dbContext.TwoFactorChallenges
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                x.ChallengeHash == hash &&
                !x.IsUsed &&
                x.ExpiresAtUtc > DateTime.UtcNow, cancellationToken);
        }

        public async Task MarkUsedAsync(TwoFactorChallengeEntity challenge, CancellationToken cancellationToken)
        {
            challenge.IsUsed = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
