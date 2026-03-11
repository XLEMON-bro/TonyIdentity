using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TonyIdentity.Database.Configurations;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Database.Context
{
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options): base(options)
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
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        }
    }
}
