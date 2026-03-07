using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Database.Context
{
    public class IdentityAppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<RefreshTokenEntity> refreshTokens { get; set; }

        public IdentityAppDbContext(DbContextOptions<IdentityAppDbContext> options) : base(options)
        {
                
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(x => x.CreatedAtUtc).IsRequired();

                entity.Property(x => x.BlockedReason).HasMaxLength(256);
            });

            builder.Entity<RefreshTokenEntity>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TokenHash).IsRequired().HasMaxLength(512);

                entity.Property(x => x.TokenSalt).IsRequired().HasMaxLength(512);

                entity.Property(x => x.CreatedByIp).HasMaxLength(128);

                entity.HasOne(x => x.User)
                    .WithMany(x=>x.RefreshTokens)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.SessionId);
            });
        }
    }
}
