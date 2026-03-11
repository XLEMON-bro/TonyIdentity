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
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(x => x.CreatedAtUtc).IsRequired();

            builder.Property(x => x.BlockedReason).HasMaxLength(512);

            builder.HasIndex(x => x.CreatedAtUtc);
            builder.HasIndex(X => X.IsBlocked);
        }
    }
}
