using Scheduler.Domain.Entities.Schedules;

namespace Scheduler.Unit.Tests.DomainTests.Entities;

public class ScheduleTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsSchedule()
    {
        var calendarId = Guid.NewGuid();
        var title = "Morning Standup";
        var description = "Daily sync";
        var startEnd = new ScheduleDate(DateTime.UtcNow.Date.AddHours(9), DateTime.UtcNow.Date.AddHours(9).AddMinutes(30));
        var recurrenceResult = RecurrencePattern.Create(RecurrenceFrequency.Weekly, interval: 1, dayOfWeek: DayOfTheWeek.Monday);
        var recurrence = recurrenceResult.AsT0;

        var result = Schedule.Create(calendarId, title, description, startEnd, false, "UTC", recurrence, null);

        result.Switch(
            s =>
            {
                Assert.Equal(calendarId, s.CalendarId);
                Assert.Equal(title, s.Title);
                Assert.Equal(description, s.Description);
                Assert.Equal(startEnd.StartDate, s.StartDate);
                Assert.Equal(startEnd.EndDate, s.EndDate);
                Assert.Equal(recurrence.Frequency, s.RecurrencePattern.Frequency);
                Assert.Equal(recurrence.Interval, s.RecurrencePattern.Interval);
                Assert.Equal(recurrence.DayOfWeek, s.RecurrencePattern.DayOfWeek);
            },
            argEx => Assert.Fail($"Expected Schedule, got ArgumentException: {argEx.Message}")
        );
    }

    [Fact]
    public void Create_WithEmptyTitle_ReturnsArgumentException()
    {
        var calendarId = Guid.NewGuid();
        var startEnd = new ScheduleDate(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var recurrenceResult = RecurrencePattern.Create(RecurrenceFrequency.Weekly, interval: 1, dayOfWeek: DayOfTheWeek.Monday);
        var recurrence = recurrenceResult.AsT0;

        var result = Schedule.Create(calendarId, " ", null, startEnd, false, null, recurrence, null);

        result.Switch(
            s => Assert.Fail("Expected ArgumentException, got Schedule"),
            argEx => Assert.IsType<ArgumentException>(argEx)
        );
    }

    [Fact]
    public void SetRecurrencePattern_WithBothCountAndEndDate_ReturnsArgumentException()
    {
        var calendarId = Guid.NewGuid();
        var startEnd = new ScheduleDate(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var recurrenceResult = RecurrencePattern.Create(RecurrenceFrequency.Weekly, interval: 1, dayOfWeek: DayOfTheWeek.Monday);
        var recurrence = recurrenceResult.AsT0;
        var schedule = Schedule.Create(calendarId, "Title", null, startEnd, false, null, recurrence, null).AsT0;

        var result = schedule.SetRecurrencePattern(RecurrenceFrequency.Weekly, 1, dayOfWeek: DayOfTheWeek.Monday, occurrenceCount: 5, recurrenceEndDate: DateTime.UtcNow.AddDays(10));

        result.Switch(
            _ => Assert.Fail("Expected ArgumentException, got Success"),
            argEx => Assert.IsType<ArgumentException>(argEx)
        );
    }

    [Fact]
    public void UpdateDetails_WithValidTitle_UpdatesProperties()
    {
        var calendarId = Guid.NewGuid();
        var startEnd = new ScheduleDate(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var recurrenceResult = RecurrencePattern.Create(RecurrenceFrequency.Weekly, interval: 1, dayOfWeek: DayOfTheWeek.Monday);
        var recurrence = recurrenceResult.AsT0;
        var schedule = Schedule.Create(calendarId, "Old Title", "Old Desc", startEnd, false, null, recurrence, null).AsT0;

        var result = schedule.UpdateDetails("New Title", "New Desc");

        result.Switch(
            _ =>
            {
                Assert.Equal("New Title", schedule.Title);
                Assert.Equal("New Desc", schedule.Description);
            },
            ex => Assert.Fail($"Expected Success, got exception: {ex.Message}")
        );
    }

    [Fact]
    public void UpdateDetails_WithEmptyTitle_ReturnsArgumentNullException()
    {
        var calendarId = Guid.NewGuid();
        var startEnd = new ScheduleDate(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var recurrenceResult = RecurrencePattern.Create(RecurrenceFrequency.Weekly, interval: 1, dayOfWeek: DayOfTheWeek.Monday);
        var recurrence = recurrenceResult.AsT0;
        var schedule = Schedule.Create(calendarId, "Old Title", null, startEnd, false, null, recurrence, null).AsT0;

        var result = schedule.UpdateDetails(" ", null);

        result.Switch(
            _ => Assert.Fail("Expected ArgumentNullException, got Success"),
            ex => Assert.IsType<ArgumentNullException>(ex)
        );
    }
}
