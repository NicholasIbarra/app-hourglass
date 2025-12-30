using Scheduler.Domain.Entities.Schedules;
using Scheduler.Domain.Managers;

namespace Scheduler.UnitTests.DomainTests.Managers;

public class ScheduledEventResolverTests
{
    private readonly ScheduledEventResolver _resolver = new();

    [Fact]
    public void Returns_pseudo_events_when_no_exceptions_exist()
    {
        // Arrange
        var start = new DateTime(2025, 01, 01, 9, 0, 0);
        var schedule = CreateDailySchedule(start);

        // Act
        var result = _resolver.Resolve(
            schedule,
            from: start,
            to: start.AddDays(2));

        // Assert
        Assert.Equal(3, result.Count);

        foreach (var evt in result)
        {
            Assert.Equal(ScheduledEventInstanceType.Pseudo, evt.Type);
            Assert.Equal(9, evt.OccursAt.Hour);
            Assert.Null(evt.EventId);
        }
    }

    [Fact]
    public void Skipped_exception_removes_occurrence()
    {
        // Arrange
        var start = new DateTime(2025, 01, 01, 9, 0, 0);
        var schedule = CreateDailySchedule(start);

        schedule.Skip(start.AddDays(1));

        // Act
        var result = _resolver.Resolve(
            schedule,
            from: start,
            to: start.AddDays(2));

        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, e => e.OccursAt == start.AddDays(1));
    }

    [Fact]
    public void Materialized_exception_returns_persisted_event()
    {
        // Arrange
        var start = new DateTime(2025, 01, 01, 9, 0, 0);
        var eventId = Guid.NewGuid();

        var schedule = CreateDailySchedule(start);
        schedule.SetSeriesEvent(start.AddDays(1), eventId);

        // Act
        var result = _resolver.Resolve(
            schedule,
            from: start,
            to: start.AddDays(2));

        // Assert
        var evt = result.Single(e => e.OccursAt == start.AddDays(1));

        Assert.Equal(ScheduledEventInstanceType.Persisted, evt.Type);
        Assert.Equal(eventId, evt.EventId);
    }

    [Fact]
    public void Rescheduled_exception_returns_persisted_event()
    {
        // Arrange
        var start = new DateTime(2025, 01, 01, 9, 0, 0);
        var eventId = Guid.NewGuid();

        var schedule = CreateDailySchedule(start);
        schedule.Reschedule(start.AddDays(2), eventId);

        // Act
        var result = _resolver.Resolve(
            schedule,
            from: start,
            to: start.AddDays(3));

        // Assert
        var evt = result.Single(e => e.OccursAt == start.AddDays(2));

        Assert.Equal(ScheduledEventInstanceType.Persisted, evt.Type);
        Assert.Equal(eventId, evt.EventId);
    }

    [Fact]
    public void Mixed_exceptions_are_resolved_correctly()
    {
        // Arrange
        var start = new DateTime(2025, 01, 01, 9, 0, 0);

        var skippedDate = start.AddDays(1);
        var materializedDate = start.AddDays(2);
        var materializedId = Guid.NewGuid();

        var schedule = CreateDailySchedule(start);
        schedule.Skip(skippedDate);
        schedule.SetSeriesEvent(materializedDate, materializedId);

        // Act
        var result = _resolver.Resolve(
            schedule,
            from: start,
            to: start.AddDays(3));

        // Assert
        Assert.Equal(3, result.Count);

        Assert.DoesNotContain(result, e => e.OccursAt == skippedDate);

        var persisted = result.Single(e => e.OccursAt == materializedDate);
        Assert.Equal(ScheduledEventInstanceType.Persisted, persisted.Type);
        Assert.Equal(materializedId, persisted.EventId);
    }

    [Fact]
    public void Does_not_generate_events_past_recurrence_end_date()
    {
        // Arrange
        var start = new DateTime(2025, 01, 01, 9, 0, 0);
        var recurrenceEnd = start.AddDays(1);

        var schedule = CreateDailySchedule(
            start,
            recurrenceEndDate: recurrenceEnd);

        // Act
        var result = _resolver.Resolve(
            schedule,
            from: start,
            to: start.AddDays(5));

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.All(e => e.OccursAt <= recurrenceEnd));
    }

    // ------------------------
    // Helpers
    // ------------------------

    private static Schedule CreateDailySchedule(
        DateTime start,
        DateTime? recurrenceEndDate = null)
    {
        var scheduleResult = Schedule.Create(
            calendarId: Guid.NewGuid(),
            title: "Test Schedule",
            description: null,
            startEndDate: new ScheduleDate(start, start.AddHours(1)),
            isAllDay: false,
            timeZone: null,
            recurrence: RecurrencePattern.Create(
                RecurrenceFrequency.Daily,
                interval: 1,
                dayOfWeek: null,
                dayOfMonth: null,
                month: null,
                occurrenceCount: null).AsT0,
            endRecurrenceDate: recurrenceEndDate);

        return scheduleResult.AsT0;
    }
}
