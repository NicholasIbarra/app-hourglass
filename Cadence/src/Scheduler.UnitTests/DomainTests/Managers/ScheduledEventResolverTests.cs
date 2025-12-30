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

    // Weekly tests

    [Fact]
    public void Weekly_no_exceptions_returns_pseudo_events()
    {
        var start = new DateTime(2025, 01, 01, 9, 0, 0); // Wednesday
        var schedule = CreateWeeklySchedule(start, DayOfTheWeek.Wednesday);

        var result = _resolver.Resolve(schedule, from: start, to: start.AddDays(21)); // 3 weeks

        // Expect occurrences at each Wednesday: 4 total including start, within 21 days: start + 7 + 14 + 21
        Assert.Equal(4, result.Count);
        Assert.All(result, e => Assert.Equal(ScheduledEventInstanceType.Pseudo, e.Type));
        Assert.All(result, e => Assert.Equal(DayOfWeek.Wednesday, e.OccursAt.DayOfWeek));
    }

    [Fact]
    public void Weekly_skipped_removes_occurrence()
    {
        var start = new DateTime(2025, 01, 01, 9, 0, 0); // Wednesday
        var schedule = CreateWeeklySchedule(start, DayOfTheWeek.Wednesday);
        var skipDate = start.AddDays(7);
        schedule.Skip(skipDate);

        var result = _resolver.Resolve(schedule, from: start, to: start.AddDays(21));

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, e => e.OccursAt == skipDate);
    }

    [Fact]
    public void Weekly_materialized_returns_persisted_event()
    {
        var start = new DateTime(2025, 01, 01, 9, 0, 0); // Wednesday
        var schedule = CreateWeeklySchedule(start, DayOfTheWeek.Wednesday);
        var materializedDate = start.AddDays(14);
        var eventId = Guid.NewGuid();
        schedule.SetSeriesEvent(materializedDate, eventId);

        var result = _resolver.Resolve(schedule, from: start, to: start.AddDays(21));

        var evt = result.Single(e => e.OccursAt == materializedDate);
        Assert.Equal(ScheduledEventInstanceType.Persisted, evt.Type);
        Assert.Equal(eventId, evt.EventId);
    }

    [Fact]
    public void Weekly_rescheduled_returns_persisted_event()
    {
        var start = new DateTime(2025, 01, 01, 9, 0, 0); // Wednesday
        var schedule = CreateWeeklySchedule(start, DayOfTheWeek.Wednesday);
        var rescheduledDate = start.AddDays(21);
        var eventId = Guid.NewGuid();
        schedule.Reschedule(rescheduledDate, eventId);

        var result = _resolver.Resolve(schedule, from: start, to: start.AddDays(21));

        var evt = result.Single(e => e.OccursAt == rescheduledDate);
        Assert.Equal(ScheduledEventInstanceType.Persisted, evt.Type);
        Assert.Equal(eventId, evt.EventId);
    }

    // Monthly tests

    [Fact]
    public void Monthly_no_exceptions_returns_pseudo_events()
    {
        var start = new DateTime(2025, 01, 15, 9, 0, 0); // 15th
        var schedule = CreateMonthlySchedule(start, dayOfMonth: 15);

        var to = new DateTime(2025, 04, 15, 9, 0, 0); // 3 months window inclusive
        var result = _resolver.Resolve(schedule, from: start, to: to);

        Assert.Equal(4, result.Count); // Jan 15, Feb 15, Mar 15, Apr 15
        Assert.All(result, e => Assert.Equal(15, e.OccursAt.Day));
        Assert.All(result, e => Assert.Equal(ScheduledEventInstanceType.Pseudo, e.Type));
    }

    [Fact]
    public void Monthly_skipped_removes_occurrence()
    {
        var start = new DateTime(2025, 01, 15, 9, 0, 0);
        var schedule = CreateMonthlySchedule(start, 15);
        var skipDate = new DateTime(2025, 02, 15, 9, 0, 0);
        schedule.Skip(skipDate);

        var result = _resolver.Resolve(schedule, from: start, to: new DateTime(2025, 04, 15, 9, 0, 0));

        Assert.Equal(3, result.Count); // Jan, Mar, Apr
        Assert.DoesNotContain(result, e => e.OccursAt == skipDate);
    }

    [Fact]
    public void Monthly_materialized_returns_persisted_event()
    {
        var start = new DateTime(2025, 01, 15, 9, 0, 0);
        var schedule = CreateMonthlySchedule(start, 15);
        var materializedDate = new DateTime(2025, 03, 15, 9, 0, 0);
        var eventId = Guid.NewGuid();
        schedule.SetSeriesEvent(materializedDate, eventId);

        var result = _resolver.Resolve(schedule, from: start, to: new DateTime(2025, 04, 15, 9, 0, 0));

        var evt = result.Single(e => e.OccursAt == materializedDate);
        Assert.Equal(ScheduledEventInstanceType.Persisted, evt.Type);
        Assert.Equal(eventId, evt.EventId);
    }

    [Fact]
    public void Monthly_rescheduled_returns_persisted_event()
    {
        var start = new DateTime(2025, 01, 15, 9, 0, 0);
        var schedule = CreateMonthlySchedule(start, 15);
        var rescheduledDate = new DateTime(2025, 04, 15, 9, 0, 0);
        var eventId = Guid.NewGuid();
        schedule.Reschedule(rescheduledDate, eventId);

        var result = _resolver.Resolve(schedule, from: start, to: rescheduledDate);

        var evt = result.Single(e => e.OccursAt == rescheduledDate);
        Assert.Equal(ScheduledEventInstanceType.Persisted, evt.Type);
        Assert.Equal(eventId, evt.EventId);
    }

    // Yearly tests

    [Fact]
    public void Yearly_no_exceptions_returns_pseudo_events()
    {
        var start = new DateTime(2025, 06, 10, 9, 0, 0);
        var schedule = CreateYearlySchedule(start, month: 6);

        var to = new DateTime(2028, 06, 10, 9, 0, 0);
        var result = _resolver.Resolve(schedule, from: start, to: to);

        Assert.Equal(4, result.Count); // 2025-06-10 .. 2028-06-10 inclusive
        Assert.All(result, e => Assert.Equal(6, e.OccursAt.Month));
        Assert.All(result, e => Assert.Equal(10, e.OccursAt.Day));
        Assert.All(result, e => Assert.Equal(ScheduledEventInstanceType.Pseudo, e.Type));
    }

    [Fact]
    public void Yearly_skipped_removes_occurrence()
    {
        var start = new DateTime(2025, 06, 10, 9, 0, 0);
        var schedule = CreateYearlySchedule(start, month: 6);
        var skipDate = new DateTime(2027, 06, 10, 9, 0, 0);
        schedule.Skip(skipDate);

        var result = _resolver.Resolve(schedule, from: start, to: new DateTime(2028, 06, 10, 9, 0, 0));

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, e => e.OccursAt == skipDate);
    }

    [Fact]
    public void Yearly_materialized_returns_persisted_event()
    {
        var start = new DateTime(2025, 06, 10, 9, 0, 0);
        var schedule = CreateYearlySchedule(start, month: 6);
        var materializedDate = new DateTime(2026, 06, 10, 9, 0, 0);
        var eventId = Guid.NewGuid();
        schedule.SetSeriesEvent(materializedDate, eventId);

        var result = _resolver.Resolve(schedule, from: start, to: new DateTime(2028, 06, 10, 9, 0, 0));

        var evt = result.Single(e => e.OccursAt == materializedDate);
        Assert.Equal(ScheduledEventInstanceType.Persisted, evt.Type);
        Assert.Equal(eventId, evt.EventId);
    }

    [Fact]
    public void Yearly_rescheduled_returns_persisted_event()
    {
        var start = new DateTime(2025, 06, 10, 9, 0, 0);
        var schedule = CreateYearlySchedule(start, month: 6);
        var rescheduledDate = new DateTime(2028, 06, 10, 9, 0, 0);
        var eventId = Guid.NewGuid();
        schedule.Reschedule(rescheduledDate, eventId);

        var result = _resolver.Resolve(schedule, from: start, to: rescheduledDate);

        var evt = result.Single(e => e.OccursAt == rescheduledDate);
        Assert.Equal(ScheduledEventInstanceType.Persisted, evt.Type);
        Assert.Equal(eventId, evt.EventId);
    }

    [Fact]
    public void Date_ranges_return_correct_number_of_occurrences_across_frequencies()
    {
        var startDaily = new DateTime(2025, 01, 01, 9, 0, 0);
        var daily = CreateDailySchedule(startDaily);
        var dailyResult = _resolver.Resolve(daily, from: startDaily, to: startDaily.AddDays(6));
        Assert.Equal(7, dailyResult.Count);

        var startWeekly = new DateTime(2025, 01, 01, 9, 0, 0); // Wednesday
        var weekly = CreateWeeklySchedule(startWeekly, DayOfTheWeek.Wednesday);
        var weeklyResult = _resolver.Resolve(weekly, from: startWeekly, to: startWeekly.AddDays(27));
        Assert.Equal(4, weeklyResult.Count); // Wednesdays on Jan 1, 8, 15, 22; Jan 29 is outside the 27-day range

        var startMonthly = new DateTime(2025, 01, 15, 9, 0, 0);
        var monthly = CreateMonthlySchedule(startMonthly, 15);
        var monthlyResult = _resolver.Resolve(monthly, from: startMonthly, to: new DateTime(2025, 06, 15, 9, 0, 0));
        Assert.Equal(6, monthlyResult.Count);

        var startYearly = new DateTime(2025, 06, 10, 9, 0, 0);
        var yearly = CreateYearlySchedule(startYearly, 6);
        var yearlyResult = _resolver.Resolve(yearly, from: startYearly, to: new DateTime(2030, 06, 10, 9, 0, 0));
        Assert.Equal(6, yearlyResult.Count);
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

    private static Schedule CreateWeeklySchedule(
        DateTime start,
        DayOfTheWeek dayOfWeek,
        int interval = 1,
        DateTime? recurrenceEndDate = null)
    {
        var scheduleResult = Schedule.Create(
            calendarId: Guid.NewGuid(),
            title: "Weekly Schedule",
            description: null,
            startEndDate: new ScheduleDate(start, start.AddHours(1)),
            isAllDay: false,
            timeZone: null,
            recurrence: RecurrencePattern.Create(
                RecurrenceFrequency.Weekly,
                interval: interval,
                dayOfWeek: dayOfWeek,
                dayOfMonth: null,
                month: null,
                occurrenceCount: null).AsT0,
            endRecurrenceDate: recurrenceEndDate);

        return scheduleResult.AsT0;
    }

    private static Schedule CreateMonthlySchedule(
        DateTime start,
        int dayOfMonth,
        int interval = 1,
        DateTime? recurrenceEndDate = null)
    {
        var scheduleResult = Schedule.Create(
            calendarId: Guid.NewGuid(),
            title: "Monthly Schedule",
            description: null,
            startEndDate: new ScheduleDate(start, start.AddHours(1)),
            isAllDay: false,
            timeZone: null,
            recurrence: RecurrencePattern.Create(
                RecurrenceFrequency.Monthly,
                interval: interval,
                dayOfWeek: null,
                dayOfMonth: dayOfMonth,
                month: null,
                occurrenceCount: null).AsT0,
            endRecurrenceDate: recurrenceEndDate);

        return scheduleResult.AsT0;
    }

    private static Schedule CreateYearlySchedule(
        DateTime start,
        int month,
        int interval = 1,
        DateTime? recurrenceEndDate = null)
    {
        var scheduleResult = Schedule.Create(
            calendarId: Guid.NewGuid(),
            title: "Yearly Schedule",
            description: null,
            startEndDate: new ScheduleDate(start, start.AddHours(1)),
            isAllDay: false,
            timeZone: null,
            recurrence: RecurrencePattern.Create(
                RecurrenceFrequency.Yearly,
                interval: interval,
                dayOfWeek: null,
                dayOfMonth: start.Day,
                month: month,
                occurrenceCount: null).AsT0,
            endRecurrenceDate: recurrenceEndDate);

        return scheduleResult.AsT0;
    }
}
