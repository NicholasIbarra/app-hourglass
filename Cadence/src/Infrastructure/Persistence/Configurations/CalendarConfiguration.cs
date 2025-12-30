using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Domain.Entities.Calendars;
using Shared.EntityFramework.Extensions;

namespace Scheduler.Infrastructure.Persistence.Configurations;

public class CalendarConfiguration : IEntityTypeConfiguration<Calendar>
{
    public void Configure(EntityTypeBuilder<Calendar> builder)
    {
        builder.ConfigureFullAudit();

        builder.ToTable("Calendar", "dbo");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Color)
            .HasMaxLength(7);
    }
}
