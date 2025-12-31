using Scheduler.Domain.Entities.CalendarEvents;

namespace Scheduler.Unit.Tests.DomainTests.Entities;

public class CalendarEventTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsCalendarEvent()
    {
        var calendarId = Guid.NewGuid();
        var title = "Meeting";
        var description = "Project sync";
        var startEnd = new EventDate(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

        var result = CalendarEvent.Create(calendarId, title, description, startEnd, false, "UTC", null);

        result.Switch(
            e =>
            {
                Assert.Equal(calendarId, e.CalendarId);
                Assert.Equal(title, e.Title);
                Assert.Equal(description, e.Description);
                Assert.Equal(startEnd.StartDate, e.StartDate);
                Assert.Equal(startEnd.EndDate, e.EndDate);
                Assert.False(e.IsAllDay);
                Assert.Equal("UTC", e.TimeZone);
                Assert.Null(e.ScheduleId);
            },
            argEx => Assert.Fail($"Expected CalendarEvent, got ArgumentException: {argEx.Message}")
        );
    }

    [Fact]
    public void Create_WithEmptyTitle_ReturnsArgumentException()
    {
        var startEnd = new EventDate(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var result = CalendarEvent.Create(Guid.NewGuid(), " ", null, startEnd, false, "UTC", null);

        result.Switch(
            e => Assert.Fail("Expected ArgumentException, got CalendarEvent"),
            argEx => Assert.IsType<ArgumentException>(argEx)
        );
    }

    [Fact]
    public void Reschedule_UpdatesDates()
    {
        var startEnd = new EventDate(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var evt = CalendarEvent.Create(Guid.NewGuid(), "Title", null, startEnd, false, "UTC", null).AsT0;

        var newDates = new EventDate(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2));
        evt.Reschedule(newDates, evt.IsAllDay, evt.TimeZone);

        Assert.Equal(newDates.StartDate, evt.StartDate);
        Assert.Equal(newDates.EndDate, evt.EndDate);
    }

    [Fact]
    public void UpdateDetails_WithEmptyTitle_ReturnsArgumentNullException()
    {
        var startEnd = new EventDate(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var evt = CalendarEvent.Create(Guid.NewGuid(), "Old", null, startEnd, false, "UTC", null).AsT0;

        var result = evt.UpdateDetails(" ", null);
        result.Switch(
            _ => Assert.Fail("Expected ArgumentNullException, got Success"),
            ex => Assert.IsType<ArgumentNullException>(ex)
        );
    }

    [Fact]
    public void UpdateDetails_WithValidTitle_UpdatesProperties()
    {
        var startEnd = new EventDate(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var evt = CalendarEvent.Create(Guid.NewGuid(), "Old", "Old D", startEnd, false, "UTC", null).AsT0;

        var result = evt.UpdateDetails("New", "New D");
        result.Switch(
            _ =>
            {
                Assert.Equal("New", evt.Title);
                Assert.Equal("New D", evt.Description);
            },
            ex => Assert.Fail($"Expected Success, got exception: {ex.Message}")
        );
    }
}
