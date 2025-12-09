using Scheduler.Domain.Entities.CalendarEvents;
using Scheduler.Domain.Entities.Schedules;

namespace Scheduler.Unit.Tests.DomainTests.Entities;

//public class EventTests
//{
//    [Fact]
//    public void Create_ValidParameters_ReturnsCalendarEvent()
//    {
//        // Arrange
//        var title = "Meeting";
//        var description = "Discuss project milestones";
//        var startEndDate = new EventDate(DateTime.Now, DateTime.Now.AddHours(1));
//        var timeZone = "Central Standard Time";

//        // Act
//        var result = CalendarEvent.Create(Guid.NewGuid(), title, description, startEndDate, false, timeZone);

//        // Assert
//        Assert.True(result.IsT0);
//        var calendarEvent = result.AsT0;
//        Assert.Equal(title, calendarEvent.Title);
//        Assert.Equal(description, calendarEvent.Description);
//        Assert.Equal(startEndDate.StartDate, calendarEvent.StartDate);
//        Assert.Equal(startEndDate.EndDate, calendarEvent.EndDate);
//        Assert.Equal(timeZone, calendarEvent.TimeZone);
//    }

//    [Fact]
//    public void Create_EmptyTitle_ReturnsArgumentException()
//    {
//        // Arrange
//        var title = "";
//        var startEndDate = new EventDate(DateTime.Now, DateTime.Now.AddHours(1));
//        var timeZone = "Central Standard Time";

//        // Act
//        var result = CalendarEvent.Create(Guid.NewGuid(), title, null, startEndDate, false, timeZone);

//        // Assert
//        Assert.True(result.IsT1);
//        Assert.IsType<ArgumentException>(result.AsT1);
//    }

//    [Fact]
//    public void AddRecurrencePattern_ValidParameters_SetsRecurrencePattern()
//    {
//        // Arrange
//        var calendarEvent = CalendarEvent.Create(Guid.NewGuid(), "Meeting", null, new EventDate(DateTime.Now, DateTime.Now.AddHours(1)), false, "Central Standard Time").AsT0;

//        // Act
//        var result = calendarEvent.AddRecurrencePattern(RecurrenceFrequency.Weekly, 1);

//        // Assert
//        Assert.True(result.IsT0);
//        Assert.NotNull(calendarEvent.RecurrencePattern);
//        Assert.Equal(RecurrenceFrequency.Weekly, calendarEvent.RecurrencePattern!.Frequency);
//        Assert.True(calendarEvent.IsRecurring);
//    }

//    [Fact]
//    public void AddRecurrencePattern_InvalidParameters_ReturnsArgumentException()
//    {
//        // Arrange
//        var calendarEvent = CalendarEvent.Create(Guid.NewGuid(), "Meeting", null, new EventDate(DateTime.Now, DateTime.Now.AddHours(1)), false, "Central Standard Time").AsT0;

//        // Act
//        var result = calendarEvent.AddRecurrencePattern(RecurrenceFrequency.Weekly, 1, occurrenceCount: -1);

//        // Assert
//        Assert.True(result.IsT1);
//        Assert.IsType<ArgumentException>(result.AsT1);
//    }

//    [Fact]
//    public void AddRecurrenceException_BeforeStartDate_ReturnsArgumentException()
//    {
//        // Arrange
//        var startEndDate = new EventDate(DateTime.Now, DateTime.Now.AddHours(1));
//        var calendarEvent = CalendarEvent.Create(Guid.NewGuid(), "Meeting", null, startEndDate, false, "Central Standard Time").AsT0;

//        // Act
//        var result = calendarEvent.AddRecurrenceException(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1));

//        // Assert
//        Assert.True(result.IsT1);
//        Assert.IsType<ArgumentException>(result.AsT1);
//    }

//    [Fact]
//    public void UpdateDetails_ValidParameters_UpdatesTitleAndDescription()
//    {
//        // Arrange
//        var calendarEvent = CalendarEvent.Create(Guid.NewGuid(), "Meeting", null, new EventDate(DateTime.Now, DateTime.Now.AddHours(1)), false, "Central Standard Time").AsT0;

//        // Act
//        var result = calendarEvent.UpdateDetails("New Meeting", "Updated description");

//        // Assert
//        Assert.True(result.IsT0);
//        Assert.Equal("New Meeting", calendarEvent.Title);
//        Assert.Equal("Updated description", calendarEvent.Description);
//    }

//    [Fact]
//    public void UpdateDetails_EmptyTitle_ReturnsArgumentNullException()
//    {
//        // Arrange
//        var calendarEvent = CalendarEvent.Create(Guid.NewGuid(), "Meeting", null, new EventDate(DateTime.Now, DateTime.Now.AddHours(1)), false, "Central Standard Time").AsT0;

//        // Act
//        var result = calendarEvent.UpdateDetails("", "Updated description");

//        // Assert
//        Assert.True(result.IsT1);
//        Assert.IsType<ArgumentNullException>(result.AsT1);
//    }

//    [Fact]
//    public void AddRecurrenceException_ShouldReturnArgumentException_WhenSeriesStartDateIsBeforeEventStartDate()
//    {
//        // Arrange
//        var startDate = DateTime.UtcNow;
//        var endDate = startDate.AddHours(1);
//        var calendarEvent = CalendarEvent.Create(Guid.NewGuid(), "Test Event", "Description", new EventDate(startDate, endDate), false, "Central Standard Time").AsT0;

//        var seriesStartDate = startDate.AddDays(-1); // Set series start date before event start date

//        // Act
//        var result = calendarEvent.AddRecurrenceException(seriesStartDate, seriesStartDate);

//        // Assert
//        Assert.True(result.IsT1);
//    }

//    [Fact]
//    public void AddRecurrenceException_ShouldReturnNone_WhenEventIsCanceled()
//    {
//        // Arrange
//        var startDate = DateTime.UtcNow;
//        var endDate = startDate.AddHours(1);
//        var calendarEvent = CalendarEvent.Create(Guid.NewGuid(), "Test Event", "Description", new EventDate(startDate, endDate), false, "Central Standard Time").AsT0;

//        var seriesStartDate = startDate.AddDays(1); // Set series start date after event start date

//        // Act
//        var result = calendarEvent.AddRecurrenceException(seriesStartDate, null, true);

//        // Assert
//        Assert.Null(result.AsT0);
//        Assert.NotEmpty(calendarEvent.RecurrenceExceptions);
//        Assert.Collection(calendarEvent.RecurrenceExceptions, re =>
//        {
//            Assert.Equal(re.SeriesCalendarEventId, calendarEvent.Id);
//            Assert.Null(re.CalendarEventId);
//        });
//    }

//    [Fact]
//    public void AddRecurrenceException_ShouldReturnNewRescheduledEvent_WhenEventIsRescheduled()
//    {
//        // Arrange
//        var startDate = DateTime.UtcNow;
//        var endDate = startDate.AddHours(1);
//        var calendarEvent = CalendarEvent.Create(Guid.NewGuid(), "Test Event", "Description", new EventDate(startDate, endDate), false, "Central Standard Time").AsT0;

//        var seriesStartDate = startDate.AddDays(1); // Set series start date after event start date

//        // Act
//        var result = calendarEvent.AddRecurrenceException(seriesStartDate, seriesStartDate.AddDays(2), false);

//        // Assert
//        Assert.True(result.IsT0); // CalendarEvent type
//        Assert.NotNull(result.AsT0);
//        Assert.Equal(seriesStartDate, result.AsT0.StartDate);
//        Assert.Collection(calendarEvent.RecurrenceExceptions, re =>
//        {
//            Assert.Equal(re.SeriesCalendarEventId, calendarEvent.Id);
//            Assert.Equal(re.CalendarEventId, result.AsT0.Id);
//        });
//    }
//}
