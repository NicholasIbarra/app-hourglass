using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrasturcture.Persistence.Configurations
{
    public class OfficeMappingConfiguration : IEntityTypeConfiguration<OfficeMapping>
    {
        public void Configure(EntityTypeBuilder<OfficeMapping> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("OfficeMapping", "dbo");
            builder.Property(x => x.OfficeCode).IsRequired().HasMaxLength(50);
            builder.Property(x => x.OfficeName).IsRequired().HasMaxLength(255);
        }
    }
}
