using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations
{
    public class AttributeTemplateItemConfiguration
    : IEntityTypeConfiguration<AttributeTemplateItem>
    {
        public void Configure(EntityTypeBuilder<AttributeTemplateItem> builder)
        {
            builder.ToTable("AttributeTemplateItems", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseIdentityColumn();

            builder.Property(x => x.AttributeName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.InputType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Text");

            builder.Property(x => x.Options)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.IsRequired)
                .HasDefaultValue(false);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            builder.HasIndex(x => x.TemplateId);
        }
    }

}
