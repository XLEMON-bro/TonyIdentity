using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Database.Configurations
{
    public class RefreshSessionEntityConfiguration : IEntityTypeConfiguration<RefreshSessionEntity>
    {
        public void Configure(EntityTypeBuilder<RefreshSessionEntity> builder)
        {
            builder.ToTable("RefreshSession");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(512);
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.UpdatedAtUtc).IsRequired();
            builder.Property(x => x.ExpiresAtUtc).IsRequired();
            builder.Property(x => x.UserAgent).HasMaxLength(1024);
            builder.Property(x => x.IpAddress).HasMaxLength(128);
            builder.Property(x => x.DeviceName).HasMaxLength(256);

            builder.HasIndex(x => x.TokenHash).IsUnique();
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.ExpiresAtUtc);

            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
