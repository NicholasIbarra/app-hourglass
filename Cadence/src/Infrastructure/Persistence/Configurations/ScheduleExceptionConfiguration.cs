using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Domain.Entities.CalendarEvents;
using Scheduler.Domain.Entities.Schedules;
using Shared.EntityFramework.Extensions;

namespace Scheduler.Infrastructure.Persistence.Configurations;

public class ScheduleExceptionConfiguration : IEntityTypeConfiguration<ScheduleException>
{
    public void Configure(EntityTypeBuilder<ScheduleException> builder)
    {
        builder.ConfigureFullAudit();

        builder.ToTable("ScheduleException", "dbo");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ScheduleId)
            .IsRequired();

        builder.Property(e => e.OriginalDate)
            .IsRequired();

        builder.Property(e => e.ExceptionType)
            .IsRequired();

        builder.HasOne<CalendarEvent>()
            .WithMany()
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
