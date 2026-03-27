using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TonyIdentity.Database.Configurations;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Database.Context
{
    public class TonyIdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public TonyIdentityDbContext(DbContextOptions<TonyIdentityDbContext> options): base(options)
        {
            
        }

        public DbSet<RefreshSessionEntity> RefreshSessions => Set<RefreshSessionEntity>();
        public DbSet<TwoFactorChallengeEntity> TwoFactorChallenges => Set<TwoFactorChallengeEntity>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new RefreshSessionEntityConfiguration());
            builder.ApplyConfiguration(new TwoFactorChallengeEntityConfiguration());
            
            ConfigureIdentityTables(builder);
        }

        private static void ConfigureIdentityTables(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            builder.Entity<IdentityRole<Guid>>().HasData(
                new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                    NormalizedName = "USER"
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                });
        }
    }
}
