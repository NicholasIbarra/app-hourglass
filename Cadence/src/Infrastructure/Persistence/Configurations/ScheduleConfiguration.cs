using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Domain.Entities.Calendars;
using Scheduler.Domain.Entities.Schedules;

namespace Scheduler.Infrastructure.Persistence.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("Schedule", "dbo");
        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.TimeZone)
            .HasMaxLength(100);

        builder.OwnsOne(s => s.RecurrencePattern, rp =>
        {
            rp.Property(r => r.Frequency)
                .IsRequired()
                .HasColumnName("Frequency");

            rp.Property(r => r.Interval)
                .IsRequired()
                .HasColumnName("Interval");

            rp.Property(r => r.DayOfWeek)
                .HasColumnName("DayOfWeek");

            rp.Property(r => r.Month)
                .HasColumnName("Month");

            rp.Property(r => r.OccurrenceCount)
                .HasColumnName("OccurrenceCount");
        });

        builder.HasMany(s => s.Exceptions)
            .WithOne()
            .HasForeignKey("ScheduleId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Calendar>()
            .WithMany()
            .HasForeignKey(s => s.CalendarId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
