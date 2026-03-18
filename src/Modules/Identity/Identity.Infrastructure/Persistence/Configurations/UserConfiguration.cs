using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            b.ToTable("Users", "identity");
            b.HasKey(u => u.Id);

            b.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            b.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            b.Property(u => u.Email).IsRequired().HasMaxLength(255);
            b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            b.Property(u => u.PhoneNumber).HasMaxLength(20);
            b.Property(u => u.AvatarUrl).HasMaxLength(500);

            // Store enums as strings
            b.Property(u => u.Role)
                .HasConversion(
                    r => r.ToString(),
                    r => Enum.Parse<UserRole>(r))
                .HasMaxLength(20)
                .IsRequired();

            b.Property(u => u.Status)
                .HasConversion(
                    s => s.ToString(),
                    s => Enum.Parse<UserStatus>(s))
                .HasMaxLength(30)
                .IsRequired();

            b.Property(u => u.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            // Soft delete global filter
            b.HasQueryFilter(u => !u.IsDeleted);

            // Indexes
            b.HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UIX_Users_Email_Active");

            b.HasIndex(u => u.Role)
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_Users_Role");

            b.HasIndex(u => u.Status)
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_Users_Status");
        }
    }

}
