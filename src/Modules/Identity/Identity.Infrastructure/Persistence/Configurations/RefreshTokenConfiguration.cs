using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> b)
        {
            b.ToTable("RefreshTokens", "identity");
            b.HasKey(t => t.Id);
            b.Property(t => t.Id).UseIdentityColumn();

            b.Property(t => t.Token).IsRequired().HasMaxLength(500);
            b.Property(t => t.RevokedReason).HasMaxLength(200);
            b.Property(t => t.ReplacedByToken).HasMaxLength(500);
            b.Property(t => t.CreatedByIp).HasMaxLength(50);

            b.HasIndex(t => t.Token)
                .IsUnique()
                .HasDatabaseName("UIX_RefreshTokens_Token");

            b.HasIndex(t => t.UserId)
                .HasDatabaseName("IX_RefreshTokens_UserId");

            // Configure relationship so EF knows principal/dependent ordering
            b.HasOne(t => t.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(t => t.UserId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);

            // No soft delete filter — tokens are revoked not deleted
        }
    }


}
