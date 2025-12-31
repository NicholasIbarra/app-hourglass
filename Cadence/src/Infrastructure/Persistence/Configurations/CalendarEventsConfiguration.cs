using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Domain.Entities.CalendarEvents;
using Scheduler.Domain.Entities.Calendars;
using Scheduler.Domain.Entities.Schedules;
using Shared.EntityFramework.Extensions;

namespace Scheduler.Infrastructure.Persistence.Configurations;

public class CalendarEventsConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ConfigureCreatedDeletedAudit();
        
        builder.ToTable("CalendarEvents");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);
        
        builder.Property(e => e.StartDate)
            .IsRequired();
        
        builder.Property(e => e.EndDate)
            .IsRequired();
        
        builder.Property(e => e.IsAllDay)
            .IsRequired();
        
        builder.Property(e => e.TimeZone)
            .HasMaxLength(100);

        builder.HasOne<Calendar>()
            .WithMany()
            .HasForeignKey(e => e.CalendarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Schedule>()
            .WithMany()
            .HasForeignKey(e => e.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
