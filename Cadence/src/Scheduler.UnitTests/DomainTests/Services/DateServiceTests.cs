using Scheduler.Domain.Entities.Schedules;
using Scheduler.Domain.Services;
using System.Globalization;

namespace Scheduler.Unit.Tests.DomainTests.Services;

public class DateServiceTests
{
    [Theory]
    [InlineData("2024-08-01", 1, "2024-08-02")] // Daily recurrence with 1-day interval
    [InlineData("2024-08-01", 2, "2024-08-03")] // Daily recurrence with 2-day interval
    [InlineData("2024-08-01", 5, "2024-08-06")] // Daily recurrence with 5-day interval
    public void GetNextOccurrence_Daily_BasicIntervals(string startDate, int interval, string expectedNextDate)
    {
        // Arrange
        var recurrencePattern = new RecurrencePattern
        {
            Frequency = RecurrenceFrequency.Daily,
            Interval = interval
        };

        var startDateTime = DateTime.Parse(startDate, CultureInfo.CurrentCulture);
        var expectedNextDateTime = DateTime.Parse(expectedNextDate, CultureInfo.CurrentCulture);

        // Act
        var nextOccurrence = DateService.GetNextOccurrence(startDateTime, recurrencePattern);

        // Assert
        Assert.Equal(expectedNextDateTime, nextOccurrence);
    }

    [Theory]
    [InlineData("2024-08-01 00:00:00", 1, "2024-08-02 00:00:00")]
    [InlineData("2024-08-01 09:15:30", 1, "2024-08-02 09:15:30")]
    [InlineData("2024-08-01 23:59:59", 2, "2024-08-03 23:59:59")]
    public void GetNextOccurrence_Daily_Preserves_TimeOfDay(string start, int interval, string expected)
    {
        var recurrencePattern = new RecurrencePattern
        {
            Frequency = RecurrenceFrequency.Daily,
            Interval = interval
        };

        var startDateTime = DateTime.Parse(start, CultureInfo.InvariantCulture);
        var expectedNext = DateTime.Parse(expected, CultureInfo.InvariantCulture);

        var next = DateService.GetNextOccurrence(startDateTime, recurrencePattern);

        Assert.Equal(expectedNext, next);
        Assert.Equal(startDateTime.TimeOfDay, next.TimeOfDay);
    }

    [Theory]
    [InlineData("2024-04-30", 1, "2024-05-01")] // 30-day month to next month
    [InlineData("2024-05-31", 1, "2024-06-01")] // 31-day month to next month
    public void GetNextOccurrence_Daily_MonthBoundary(string startDate, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Daily, Interval = interval };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(expected, next);
    }

    [Theory]
    [InlineData("2024-02-28", 1, "2024-02-29")] // Leap day exists
    [InlineData("2024-02-29", 1, "2024-03-01")] // Day after leap day
    [InlineData("2024-02-28", 2, "2024-03-01")] // Crossing through leap day with 2-day interval
    [InlineData("2025-02-28", 1, "2025-03-01")] // Non-leap year, next day is March 1
    public void GetNextOccurrence_Daily_LeapYear(string startDate, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Daily, Interval = interval };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(expected, next);
    }

    [Theory]
    [InlineData("2024-12-31", 1, "2025-01-01")]
    [InlineData("2024-12-30", 3, "2025-01-02")]
    public void GetNextOccurrence_Daily_YearBoundary(string startDate, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Daily, Interval = interval };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(expected, next);
    }

    [Theory]
    [InlineData("2024-01-01", 1000, "2026-09-27")] // 1000 days later
    public void GetNextOccurrence_Daily_LargeInterval(string startDate, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Daily, Interval = interval };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(expected, next);
    }

    [Fact]
    public void GetNextOccurrence_Daily_Preserves_DateTimeKind_Unspecified()
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Daily, Interval = 1 };
        var start = new DateTime(2024, 08, 01, 10, 0, 0, DateTimeKind.Unspecified);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(DateTimeKind.Unspecified, next.Kind);
        Assert.Equal(start.TimeOfDay, next.TimeOfDay);
    }

    [Fact]
    public void GetNextOccurrence_Daily_Preserves_DateTimeKind_Utc()
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Daily, Interval = 1 };
        var start = new DateTime(2024, 08, 01, 10, 0, 0, DateTimeKind.Utc);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(DateTimeKind.Utc, next.Kind);
        Assert.Equal(start.AddDays(1), next);
    }

    [Fact]
    public void GetNextOccurrence_Daily_Preserves_DateTimeKind_Local()
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Daily, Interval = 1 };
        var startLocal = new DateTime(2024, 08, 01, 10, 0, 0, DateTimeKind.Local);

        var next = DateService.GetNextOccurrence(startLocal, pat);

        Assert.Equal(DateTimeKind.Local, next.Kind);
        Assert.Equal(startLocal.AddDays(1), next);
    }


    [Fact]
    public void GetNextOccurrence_Daily_Overflow_Throws()
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Daily, Interval = 2 };
        var nearMax = DateTime.MaxValue.AddDays(-1); // one day before MaxValue

        Assert.Throws<ArgumentOutOfRangeException>(() => DateService.GetNextOccurrence(nearMax, pat));
    }

    [Theory]
    [InlineData("2024-08-01", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, 1, "2024-08-05")] // Thu -> next Mon
    [InlineData("2024-08-01", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, 2, "2024-08-12")] // Thu -> bi-weekly -> Mon 8/12
    [InlineData("2024-08-01", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 1, "2024-08-02")] // Thu -> Fri
    [InlineData("2024-08-01", DayOfTheWeek.Monday | DayOfTheWeek.Friday, 2, "2024-08-02")] // Thu -> Fri (same week), interval=2 doesn't matter yet
    [InlineData("2024-08-02", DayOfTheWeek.Monday | DayOfTheWeek.Friday, 2, "2024-08-12")] // Fri (match day) -> next is Mon 8/12 (strictly after & bi-weekly)
    public void GetNextOccurrence_Weekly_Test(string startDate, DayOfTheWeek dayOfTheWeek, int interval, string expectedNextDate)
    {
        // Arrange
        var recurrencePattern = new RecurrencePattern
        {
            Frequency = RecurrenceFrequency.Weekly,
            Interval = interval,
            DayOfWeek = dayOfTheWeek
        };

        var startDateTime = DateTime.Parse(startDate, CultureInfo.CurrentCulture);
        var expectedNextDateTime = DateTime.Parse(expectedNextDate, CultureInfo.CurrentCulture);

        // Act
        var nextOccurrence = DateService.GetNextOccurrence(startDateTime, recurrencePattern);

        // Assert
        Assert.Equal(expectedNextDateTime, nextOccurrence);
    }

    [Theory]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday, 3, "2024-08-19 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday, 1, "2024-08-06 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday, 2, "2024-08-13 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday, 3, "2024-08-20 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday, 3, "2024-08-19 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday, 1, "2024-08-07 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday, 2, "2024-08-14 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday, 3, "2024-08-21 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, 3, "2024-08-19 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 1, "2024-08-06 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 2, "2024-08-13 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 3, "2024-08-20 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 3, "2024-08-19 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday, 1, "2024-08-08 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday, 2, "2024-08-15 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday, 3, "2024-08-22 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday, 3, "2024-08-19 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 1, "2024-08-06 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 2, "2024-08-13 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 3, "2024-08-20 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 3, "2024-08-19 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 1, "2024-08-07 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 2, "2024-08-14 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 3, "2024-08-21 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 3, "2024-08-19 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 1, "2024-08-06 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 2, "2024-08-13 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 3, "2024-08-20 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 3, "2024-08-19 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday, 3, "2024-08-18 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Friday, 2, "2024-08-09 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Friday, 3, "2024-08-16 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Saturday, 2, "2024-08-10 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Saturday, 3, "2024-08-17 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 2, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Saturday, 3, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 2, "2024-08-02 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday | DayOfTheWeek.Monday | DayOfTheWeek.Tuesday | DayOfTheWeek.Wednesday | DayOfTheWeek.Thursday | DayOfTheWeek.Friday | DayOfTheWeek.Saturday, 3, "2024-08-02 09:30:00")]

    public void GetNextOccurrence_Weekly_AllFlagCombos_Hardcoded(string startDate, DayOfTheWeek flags, int interval, string expectedNextDate)
    {
        // Arrange
        var recurrencePattern = new RecurrencePattern
        {
            Frequency = RecurrenceFrequency.Weekly,
            Interval = interval,
            DayOfWeek = flags
        };

        var startDateTime = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);

        // Act
        var nextOccurrence = DateService.GetNextOccurrence(startDateTime, recurrencePattern);

        // Assert
        Assert.Equal(expected, nextOccurrence);
        Assert.Equal(startDateTime.TimeOfDay, nextOccurrence.TimeOfDay); // time-of-day preserved
    }


    // Time-of-day preserved and "strictly-after" same day (e.g., Monday 09:30 with flags including Monday goes to next flag day)
    [Theory]
    [InlineData("2024-08-05 09:30:00", DayOfTheWeek.Monday | DayOfTheWeek.Wednesday, 1, "2024-08-07 09:30:00")] // Monday morning -> Wednesday (not same-day)
    [InlineData("2024-08-07 23:59:59", DayOfTheWeek.Wednesday | DayOfTheWeek.Friday, 1, "2024-08-09 23:59:59")] // Late Wed -> Fri
    public void GetNextOccurrence_Weekly_Preserves_TimeOfDay_And_StrictlyAfter(
        string start, DayOfTheWeek flags, int interval, string expected)
    {
        var pat = new RecurrencePattern
        {
            Frequency = RecurrenceFrequency.Weekly,
            Interval = interval,
            DayOfWeek = flags
        };

        var startDt = DateTime.Parse(start, CultureInfo.InvariantCulture);
        var expectedDt = DateTime.Parse(expected, CultureInfo.InvariantCulture);

        var next = DateService.GetNextOccurrence(startDt, pat);

        Assert.Equal(expectedDt, next);
        Assert.Equal(startDt.TimeOfDay, next.TimeOfDay);
    }

    // Week boundary: Saturday -> next week's Monday (or nearest flag); bi-weekly should hop another week
    [Theory]
    [InlineData("2024-08-03", DayOfTheWeek.Monday, 1, "2024-08-05")] // Sat -> Mon
    [InlineData("2024-08-03", DayOfTheWeek.Monday, 2, "2024-08-12")] // Sat -> bi-weekly -> Mon 8/12
    public void GetNextOccurrence_Weekly_WeekBoundary(string startDate, DayOfTheWeek flags, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DayOfWeek = flags };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(expected, next);
    }

    // Year boundary: end of year rolling to next year's flagged day(s)
    [Theory]
    [InlineData("2024-12-31", DayOfTheWeek.Wednesday, 1, "2025-01-01")] // Tue -> Wed (next day is new year)
    [InlineData("2024-12-27", DayOfTheWeek.Monday, 2, "2025-01-06")] // Fri -> bi-weekly -> Mon 1/6/2025
    public void GetNextOccurrence_Weekly_YearBoundary(string startDate, DayOfTheWeek flags, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DayOfWeek = flags };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(expected, next);
    }

    // Single-day flags
    [Theory]
    [InlineData("2024-08-01", DayOfTheWeek.Sunday, 1, "2024-08-04")] // Thu -> Sun
    [InlineData("2024-08-01", DayOfTheWeek.Sunday, 2, "2024-08-11")] // Thu -> Sun (2-week cycle)
    [InlineData("2024-08-01", DayOfTheWeek.Sunday, 3, "2024-08-18")] // Thu -> Sun (3-week cycle)

    [InlineData("2024-08-01", DayOfTheWeek.Monday, 1, "2024-08-05")] // Thu -> Mon
    [InlineData("2024-08-01", DayOfTheWeek.Monday, 2, "2024-08-12")] // Thu -> Mon (2-week cycle)
    [InlineData("2024-08-01", DayOfTheWeek.Monday, 3, "2024-08-19")] // Thu -> Mon (3-week cycle)

    [InlineData("2024-08-01", DayOfTheWeek.Tuesday, 1, "2024-08-06")] // Thu -> Tue
    [InlineData("2024-08-01", DayOfTheWeek.Tuesday, 2, "2024-08-13")] // Thu -> Tue (2-week cycle)
    [InlineData("2024-08-01", DayOfTheWeek.Tuesday, 3, "2024-08-20")] // Thu -> Tue (3-week cycle)

    [InlineData("2024-08-01", DayOfTheWeek.Wednesday, 1, "2024-08-07")] // Thu -> Wed
    [InlineData("2024-08-01", DayOfTheWeek.Wednesday, 2, "2024-08-14")] // Thu -> Wed (2-week cycle)
    [InlineData("2024-08-01", DayOfTheWeek.Wednesday, 3, "2024-08-21")] // Thu -> Wed (3-week cycle)

    [InlineData("2024-08-01", DayOfTheWeek.Thursday, 1, "2024-08-08")] // Thu -> next Thu
    [InlineData("2024-08-01", DayOfTheWeek.Thursday, 2, "2024-08-15")] // Thu -> next Thu (2-week cycle)
    [InlineData("2024-08-01", DayOfTheWeek.Thursday, 3, "2024-08-22")] // Thu -> next Thu (3-week cycle)

    [InlineData("2024-08-01", DayOfTheWeek.Friday, 1, "2024-08-02")] // Thu -> Fri
    [InlineData("2024-08-01", DayOfTheWeek.Friday, 2, "2024-08-09")] // Thu -> Fri (2-week cycle)
    [InlineData("2024-08-01", DayOfTheWeek.Friday, 3, "2024-08-16")] // Thu -> Fri (3-week cycle)

    [InlineData("2024-08-01", DayOfTheWeek.Saturday, 1, "2024-08-03")] // Thu -> Sat
    [InlineData("2024-08-01", DayOfTheWeek.Saturday, 2, "2024-08-10")] // Thu -> Sat (2-week cycle)
    [InlineData("2024-08-01", DayOfTheWeek.Saturday, 3, "2024-08-17")] // Thu -> Sat (3-week cycle)
    public void GetNextOccurrence_Weekly_SingleDay(string startDate, DayOfTheWeek flag, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DayOfWeek = flag };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(expected, next);
    }

    [Theory]
    // Start on SUNDAY (2024-07-28) — strictly-after -> 2024-08-04
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Sunday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Sunday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Sunday, 3, "2024-08-18 09:30:00")]

    // Start on MONDAY (2024-07-29)
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Sunday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Sunday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Sunday, 3, "2024-08-18 09:30:00")]

    // Start on TUESDAY (2024-07-30)
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Sunday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Sunday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Sunday, 3, "2024-08-18 09:30:00")]

    // Start on WEDNESDAY (2024-07-31)
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Sunday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Sunday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Sunday, 3, "2024-08-18 09:30:00")]

    // Start on THURSDAY (2024-08-01)
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Sunday, 3, "2024-08-18 09:30:00")]

    // Start on FRIDAY (2024-08-02)
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Sunday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Sunday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Sunday, 3, "2024-08-18 09:30:00")]

    // Start on SATURDAY (2024-08-03)
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Sunday, 1, "2024-08-04 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Sunday, 2, "2024-08-11 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Sunday, 3, "2024-08-18 09:30:00")]
    public void GetNextOccurrence_Weekly_SingleFlag_Sunday_AllStartDays(
        string startDate, DayOfTheWeek flags, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DayOfWeek = flags };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);
        var next = DateService.GetNextOccurrence(start, pat);
        Assert.Equal(expected, next);
        Assert.Equal(start.TimeOfDay, next.TimeOfDay);
    }

    [Theory]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Monday, 1, "2024-07-29 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Monday, 2, "2024-08-05 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Monday, 3, "2024-08-12 09:30:00")]

    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Monday, 1, "2024-08-05 09:30:00")] // strictly-after
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Monday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Monday, 3, "2024-08-19 09:30:00")]

    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Monday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Monday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Monday, 3, "2024-08-19 09:30:00")]

    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Monday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Monday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Monday, 3, "2024-08-19 09:30:00")]

    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Monday, 3, "2024-08-19 09:30:00")]

    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Monday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Monday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Monday, 3, "2024-08-19 09:30:00")]

    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Monday, 1, "2024-08-05 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Monday, 2, "2024-08-12 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Monday, 3, "2024-08-19 09:30:00")]
    public void GetNextOccurrence_Weekly_SingleFlag_Monday_AllStartDays(
        string startDate, DayOfTheWeek flags, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DayOfWeek = flags };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);
        var next = DateService.GetNextOccurrence(start, pat);
        Assert.Equal(expected, next);
        Assert.Equal(start.TimeOfDay, next.TimeOfDay);
    }

    [Theory]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Tuesday, 1, "2024-07-30 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Tuesday, 2, "2024-08-06 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Tuesday, 3, "2024-08-13 09:30:00")]

    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Tuesday, 1, "2024-07-30 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Tuesday, 2, "2024-08-06 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Tuesday, 3, "2024-08-13 09:30:00")]

    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Tuesday, 1, "2024-08-06 09:30:00")] // strictly-after
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Tuesday, 2, "2024-08-13 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Tuesday, 3, "2024-08-20 09:30:00")]

    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Tuesday, 1, "2024-08-06 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Tuesday, 2, "2024-08-13 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Tuesday, 3, "2024-08-20 09:30:00")]

    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday, 1, "2024-08-06 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday, 2, "2024-08-13 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Tuesday, 3, "2024-08-20 09:30:00")]

    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Tuesday, 1, "2024-08-06 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Tuesday, 2, "2024-08-13 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Tuesday, 3, "2024-08-20 09:30:00")]

    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Tuesday, 1, "2024-08-06 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Tuesday, 2, "2024-08-13 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Tuesday, 3, "2024-08-20 09:30:00")]
    public void GetNextOccurrence_Weekly_SingleFlag_Tuesday_AllStartDays(
        string startDate, DayOfTheWeek flags, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DayOfWeek = flags };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);
        var next = DateService.GetNextOccurrence(start, pat);
        Assert.Equal(expected, next);
        Assert.Equal(start.TimeOfDay, next.TimeOfDay);
    }

    [Theory]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Wednesday, 1, "2024-07-31 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Wednesday, 2, "2024-08-07 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Wednesday, 3, "2024-08-14 09:30:00")]

    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Wednesday, 1, "2024-07-31 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Wednesday, 2, "2024-08-07 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Wednesday, 3, "2024-08-14 09:30:00")]

    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Wednesday, 1, "2024-07-31 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Wednesday, 2, "2024-08-07 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Wednesday, 3, "2024-08-14 09:30:00")]

    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Wednesday, 1, "2024-08-07 09:30:00")] // strictly-after
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Wednesday, 2, "2024-08-14 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Wednesday, 3, "2024-08-21 09:30:00")]

    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday, 1, "2024-08-07 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday, 2, "2024-08-14 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Wednesday, 3, "2024-08-21 09:30:00")]

    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Wednesday, 1, "2024-08-07 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Wednesday, 2, "2024-08-14 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Wednesday, 3, "2024-08-21 09:30:00")]

    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Wednesday, 1, "2024-08-07 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Wednesday, 2, "2024-08-14 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Wednesday, 3, "2024-08-21 09:30:00")]
    public void GetNextOccurrence_Weekly_SingleFlag_Wednesday_AllStartDays(
        string startDate, DayOfTheWeek flags, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DayOfWeek = flags };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);
        var next = DateService.GetNextOccurrence(start, pat);
        Assert.Equal(expected, next);
        Assert.Equal(start.TimeOfDay, next.TimeOfDay);
    }

    [Theory]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Thursday, 1, "2024-08-01 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Thursday, 2, "2024-08-08 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Thursday, 3, "2024-08-15 09:30:00")]

    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Thursday, 1, "2024-08-01 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Thursday, 2, "2024-08-08 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Thursday, 3, "2024-08-15 09:30:00")]

    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Thursday, 1, "2024-08-01 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Thursday, 2, "2024-08-08 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Thursday, 3, "2024-08-15 09:30:00")]

    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Thursday, 1, "2024-08-01 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Thursday, 2, "2024-08-08 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Thursday, 3, "2024-08-15 09:30:00")]

    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday, 1, "2024-08-08 09:30:00")] // strictly-after
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday, 2, "2024-08-15 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Thursday, 3, "2024-08-22 09:30:00")]

    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Thursday, 1, "2024-08-08 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Thursday, 2, "2024-08-15 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Thursday, 3, "2024-08-22 09:30:00")]

    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Thursday, 1, "2024-08-08 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Thursday, 2, "2024-08-15 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Thursday, 3, "2024-08-22 09:30:00")]
    public void GetNextOccurrence_Weekly_SingleFlag_Thursday_AllStartDays(
        string startDate, DayOfTheWeek flags, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DayOfWeek = flags };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);
        var next = DateService.GetNextOccurrence(start, pat);
        Assert.Equal(expected, next);
        Assert.Equal(start.TimeOfDay, next.TimeOfDay);
    }


    [Theory]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Friday, 2, "2024-08-09 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Friday, 3, "2024-08-16 09:30:00")]

    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Friday, 2, "2024-08-09 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Friday, 3, "2024-08-16 09:30:00")]

    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Friday, 2, "2024-08-09 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Friday, 3, "2024-08-16 09:30:00")]

    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Friday, 2, "2024-08-09 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Friday, 3, "2024-08-16 09:30:00")]

    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Friday, 1, "2024-08-02 09:30:00")] // Thu -> Fri
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Friday, 2, "2024-08-09 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Friday, 3, "2024-08-16 09:30:00")]

    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Friday, 1, "2024-08-09 09:30:00")] // strictly-after
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Friday, 2, "2024-08-16 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Friday, 3, "2024-08-23 09:30:00")]

    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Friday, 1, "2024-08-09 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Friday, 2, "2024-08-16 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Friday, 3, "2024-08-23 09:30:00")]
    public void GetNextOccurrence_Weekly_SingleFlag_Friday_AllStartDays(
        string startDate, DayOfTheWeek flags, int interval, string expectedNextDate)
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = interval, DayOfWeek = flags };
        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);
        var next = DateService.GetNextOccurrence(start, pat);
        Assert.Equal(expected, next);
        Assert.Equal(start.TimeOfDay, next.TimeOfDay);
    }

    // DateTimeKind propagation (AddDays preserves Kind)
    [Fact]
    public void GetNextOccurrence_Weekly_Preserves_Kind_Unspecified()
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = 1, DayOfWeek = DayOfTheWeek.Monday };
        var start = new DateTime(2024, 08, 01, 10, 00, 00, DateTimeKind.Unspecified);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(DateTimeKind.Unspecified, next.Kind);
    }

    [Fact]
    public void GetNextOccurrence_Weekly_Preserves_Kind_Utc()
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = 1, DayOfWeek = DayOfTheWeek.Monday };
        var start = new DateTime(2024, 08, 01, 10, 00, 00, DateTimeKind.Utc);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(DateTimeKind.Utc, next.Kind);
    }

    [Fact]
    public void GetNextOccurrence_Weekly_Preserves_Kind_Local()
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = 1, DayOfWeek = DayOfTheWeek.Monday };
        var start = new DateTime(2024, 08, 01, 10, 00, 00, DateTimeKind.Local);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(DateTimeKind.Local, next.Kind);
    }

    [Fact]
    public void GetNextOccurrence_Weekly_MissingDayOfWeek_ShouldThrow()
    {
        var pat = new RecurrencePattern { Frequency = RecurrenceFrequency.Weekly, Interval = 1, DayOfWeek = null };
        var start = new DateTime(2024, 08, 01);

        Assert.Throws<InvalidOperationException>(() => DateService.GetNextOccurrence(start, pat));
    }

    [Theory]
    // Start on SUNDAY (2024-07-28) → next Sat 2024-08-03; bi/tri-weekly skip whole weeks
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Saturday, 2, "2024-08-10 09:30:00")]
    [InlineData("2024-07-28 09:30:00", DayOfTheWeek.Saturday, 3, "2024-08-17 09:30:00")]

    // Start on MONDAY (2024-07-29) → next Sat 2024-08-03
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Saturday, 2, "2024-08-10 09:30:00")]
    [InlineData("2024-07-29 09:30:00", DayOfTheWeek.Saturday, 3, "2024-08-17 09:30:00")]

    // Start on TUESDAY (2024-07-30)
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Saturday, 2, "2024-08-10 09:30:00")]
    [InlineData("2024-07-30 09:30:00", DayOfTheWeek.Saturday, 3, "2024-08-17 09:30:00")]

    // Start on WEDNESDAY (2024-07-31)
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Saturday, 2, "2024-08-10 09:30:00")]
    [InlineData("2024-07-31 09:30:00", DayOfTheWeek.Saturday, 3, "2024-08-17 09:30:00")]

    // Start on THURSDAY (2024-08-01)
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Saturday, 2, "2024-08-10 09:30:00")]
    [InlineData("2024-08-01 09:30:00", DayOfTheWeek.Saturday, 3, "2024-08-17 09:30:00")]

    // Start on FRIDAY (2024-08-02)
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Saturday, 1, "2024-08-03 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Saturday, 2, "2024-08-10 09:30:00")]
    [InlineData("2024-08-02 09:30:00", DayOfTheWeek.Saturday, 3, "2024-08-17 09:30:00")]

    // Start on SATURDAY (2024-08-03) — strictly-after → next Sat (skip same-day)
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Saturday, 1, "2024-08-10 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Saturday, 2, "2024-08-17 09:30:00")]
    [InlineData("2024-08-03 09:30:00", DayOfTheWeek.Saturday, 3, "2024-08-24 09:30:00")]

    // Late-night edge: start late Sat -> next Sat preserves time
    [InlineData("2024-08-03 23:59:59", DayOfTheWeek.Saturday, 1, "2024-08-10 23:59:59")]
    public void GetNextOccurrence_Weekly_SingleFlag_Saturday_AllStartDays(
       string startDate,
       DayOfTheWeek flags,
       int interval,
       string expectedNextDate)
    {
        var pat = new RecurrencePattern
        {
            Frequency = RecurrenceFrequency.Weekly,
            Interval = interval,
            DayOfWeek = flags
        };

        var start = DateTime.Parse(startDate, CultureInfo.InvariantCulture);
        var expected = DateTime.Parse(expectedNextDate, CultureInfo.InvariantCulture);

        var next = DateService.GetNextOccurrence(start, pat);

        Assert.Equal(expected, next);
        Assert.Equal(start.TimeOfDay, next.TimeOfDay); // time-of-day preserved
    }

    [Theory]
    [InlineData("2024-08-01", 1, "2024-09-01")] // Monthly recurrence with 1-month interval
    [InlineData("2024-08-01", 2, "2024-10-01")] // Monthly recurrence with 2-month interval
    [InlineData("2024-08-01", 6, "2025-02-01")] // Monthly recurrence with 6-month interval
    public void GetNextOccurrence_Monthly_Test(string startDate, int interval, string expectedNextDate)
    {
        // Arrange
        var recurrencePattern = new RecurrencePattern
        {
            Frequency = RecurrenceFrequency.Monthly,
            Interval = interval,
            DayOfMonth = 1
        };

        var startDateTime = DateTime.Parse(startDate, CultureInfo.CurrentCulture);
        var expectedNextDateTime = DateTime.Parse(expectedNextDate, CultureInfo.CurrentCulture);

        // Act
        var nextOccurrence = DateService.GetNextOccurrence(startDateTime, recurrencePattern);

        // Assert
        Assert.Equal(expectedNextDateTime, nextOccurrence);
    }

    [Theory]
    [InlineData("2024-08-01", 1, "2025-08-01")] // Yearly recurrence with 1-year interval
    [InlineData("2024-08-01", 2, "2026-08-01")] // Yearly recurrence with 2-year interval
    [InlineData("2024-08-01", 5, "2029-08-01")] // Yearly recurrence with 5-year interval
    public void GetNextOccurrence_Yearly_Test(string startDate, int interval, string expectedNextDate)
    {
        // Arrange
        var recurrencePattern = new RecurrencePattern
        {
            Frequency = RecurrenceFrequency.Yearly,
            Interval = interval,
            Month = 8,
            DayOfMonth = 1
        };

        var startDateTime = DateTime.Parse(startDate, CultureInfo.CurrentCulture);
        var expectedNextDateTime = DateTime.Parse(expectedNextDate, CultureInfo.CurrentCulture);

        // Act
        var nextOccurrence = DateService.GetNextOccurrence(startDateTime, recurrencePattern);

        // Assert
        Assert.Equal(expectedNextDateTime, nextOccurrence);
    }

}
