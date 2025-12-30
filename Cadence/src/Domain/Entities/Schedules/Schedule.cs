using OneOf;
using OneOf.Types;
using Scheduler.Domain.Services;
using Shared.Entities;

namespace Scheduler.Domain.Entities.Schedules;

public class Schedule : BaseEntity, IAggregateRoot
{
    public Guid CalendarId { get; internal set; }

    public string Title { get; internal set; } = "";

    public string? Description { get; internal set; }

    public DateTime StartDate { get; internal set; }

    public DateTime EndDate { get; internal set; }

    public string? TimeZone { get; internal set; }

    public bool IsAllDay { get; internal set; }

    public RecurrencePattern RecurrencePattern { get; internal set; } = null!;

    public DateTime? RecurrenceEndDate { get; internal set; }

    public IReadOnlyCollection<ScheduleException> Exceptions => _exceptions.AsReadOnly();
    private List<ScheduleException> _exceptions = new List<ScheduleException>();

    public static OneOf<Schedule, ArgumentException> Create(
        Guid calendarId,
        string title,
        string? description,
        ScheduleDate startEndDate,
        bool isAllDay,
        string? timeZone,
        RecurrencePattern recurrence,
        DateTime? endRecurrenceDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return new ArgumentException("Title is required", nameof(title));
        }

        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            CalendarId = calendarId,
            Title = title,
            Description = description,
            StartDate = startEndDate.StartDate,
            EndDate = startEndDate.EndDate,
            IsAllDay = isAllDay,
            TimeZone = timeZone,
            RecurrenceEndDate = endRecurrenceDate,

            _exceptions = new List<ScheduleException>(),
        };

        var result = schedule.SetRecurrencePattern(
            recurrence.Frequency,
            recurrence.Interval,
            recurrence.DayOfWeek,
            recurrence.DayOfMonth,
            recurrence.Month,
            recurrence.OccurrenceCount,
            endRecurrenceDate);

        if (result.IsT1)
        {
            return result.AsT1;
        }

        return schedule;
    }

    public OneOf<Success, ArgumentException> SetRecurrencePattern(
        RecurrenceFrequency frequency,
        int interval,
        DayOfTheWeek? dayOfWeek = null,
        int? dayOfMonth = null,
        int? month = null,
        int? occurrenceCount = null,
        DateTime? recurrenceEndDate = null)
    {
        if (occurrenceCount is not null && recurrenceEndDate is not null)
        {
            return new ArgumentException("Cannot specify both an occurrence count and a recurrence end date");
        }

        if (occurrenceCount is not null && occurrenceCount <= 0)
        {
            return new ArgumentException("Occurrence count must be greater than 0");
        }

        if (recurrenceEndDate is not null && recurrenceEndDate < StartDate)
        {
            return new ArgumentException("Recurrence end date cannot be before the CalendarEvent start date");
        }

        var recurrence = RecurrencePattern.Create(
            frequency,
            interval,
            dayOfWeek,
            dayOfMonth,
            month,
            occurrenceCount);

        if (recurrence.IsT1)
        {
            return recurrence.AsT1;
        }

        RecurrencePattern = recurrence.AsT0;

        if (occurrenceCount is not null)
        {
            var endDate = StartDate;

            while (frequency == RecurrenceFrequency.Weekly && !RecurrencePattern.DayOfWeek!.Value.HasFlag(DateService.ToDayOfWeekFlag(endDate.DayOfWeek)))
            {
                endDate = DateService.GetNextOccurrence(endDate, RecurrencePattern);
            }

            // do while loop to get the next occurrence until the occurrence count is reached
            // subtract 1 from the occurrence count since the first occurrence is the original CalendarEvent
            var count = occurrenceCount.Value - 1;

            do
            {
                endDate = DateService.GetNextOccurrence(endDate, RecurrencePattern!);
                count--;
            } while (count > 0);

            recurrenceEndDate = endDate;
        }

        RecurrenceEndDate = recurrenceEndDate;

        return new Success();
    }

    public OneOf<Success, ArgumentException> UpdateSeries(ScheduleDate startEndDate, bool isAllDay, string? timeZone)
    {
        if (startEndDate.StartDate >= startEndDate.EndDate)
        {
            return new ArgumentException("Start date must be before end date");
        }

        StartDate = IsAllDay ? startEndDate.StartDate.Date : startEndDate.StartDate;
        EndDate = IsAllDay ? startEndDate.EndDate.Date : startEndDate.EndDate;
        IsAllDay = isAllDay;
        TimeZone = timeZone;

        return new Success();
    }

    public OneOf<Success, ArgumentException> Skip(DateTime startDate)
    {
        if (startDate < StartDate)
        {
            return new ArgumentException("Series date cannot be before the CalendarEvent start date", nameof(startDate));
        }

        var exception = ScheduleException.Skip(Id, startDate);
        _exceptions.Add(exception);

        return new Success();
    }

    public OneOf<Success, ArgumentException> Reschedule(DateTime startDate, Guid rescheduledEventId)
    {
        if (startDate < StartDate)
        {
            return new ArgumentException("Series date cannot be before the CalendarEvent start date", nameof(startDate));
        }

        var exception = ScheduleException.Reschedule(Id, startDate, rescheduledEventId);
        _exceptions.Add(exception);

        return new Success();
    }

    public OneOf<Success, ArgumentException> SetSeriesEvent(DateTime startDate, Guid createdEventId)
    {
        if (startDate < StartDate)
        {
            return new ArgumentException("Series date cannot be before the CalendarEvent start date", nameof(startDate));
        }

        var exception = ScheduleException.Materialize(Id, startDate, createdEventId);
        _exceptions.Add(exception);

        return new Success();
    }

    public OneOf<Success, ArgumentNullException> UpdateDetails(string newTitle, string? newDescription)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            return new ArgumentNullException(nameof(newTitle), "Title is required");
        }

        Title = newTitle;
        Description = newDescription;

        return new Success();
    }
}
