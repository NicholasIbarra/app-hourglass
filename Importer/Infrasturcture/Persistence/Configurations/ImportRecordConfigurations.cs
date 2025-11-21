using Domain.Imports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrasturcture.Persistence.Configurations
{
    public class ImportRecordConfigurations : IEntityTypeConfiguration<ImportRecord>
    {
        public void Configure(EntityTypeBuilder<ImportRecord> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("ImportRecord", "dbo");
            builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>();
            builder.Property(x => x.TotalRecords).IsRequired();
            builder.Property(x => x.ProcessedRecords).IsRequired();
            builder.Property(x => x.ErrorMessage).IsRequired(false);
        }
    }
}
