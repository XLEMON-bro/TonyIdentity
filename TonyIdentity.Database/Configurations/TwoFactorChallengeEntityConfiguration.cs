using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Database.Configurations
{
    public class TwoFactorChallengeEntityConfiguration : IEntityTypeConfiguration<TwoFactorChallengeEntity>
    {
        public void Configure(EntityTypeBuilder<TwoFactorChallengeEntity> builder)
        {
            builder.ToTable("TwoFactorChallenge");

            builder.HasKey(x => x.id);

            builder.Property(x => x.ChallengeTokenHash).IsRequired().HasMaxLength(512);
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.ExpiresAtUtc).IsRequired();
            builder.Property(x => x.UserAgent).HasMaxLength(1024);
            builder.Property(x => x.IpAddress).HasMaxLength(128);

            builder.HasIndex(x => x.ChallengeTokenHash).IsUnique();
            builder.HasIndex(x => x.UserId);

            builder.HasIndex(x => x.ExpiresAtUtc);

            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
