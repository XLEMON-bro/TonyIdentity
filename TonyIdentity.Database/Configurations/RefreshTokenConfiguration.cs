using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TonyIdentity.Database.Entities;

namespace TonyIdentity.Database.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedByIp).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RevokedByIp).HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReplacedByToken)
            .WithOne(x => x.ReplacedToken)
            .HasForeignKey<RefreshToken>(x => x.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
