using OneOf;
using OneOf.Types;
using Shared.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Scheduler.Domain.Entities.CalendarEvents;

public class CalendarEvent : BaseEntity, IAggregateRoot
{
    internal CalendarEvent() { }

    public Guid CalendarId { get; internal set; }

    public Guid? ScheduleId { get; internal set; }

    public string Title { get; internal set; } = "";

    public string? Description { get; internal set; }

    public DateTime StartDate { get; internal set; }

    public DateTime EndDate { get; internal set; }

    public string? TimeZone { get; internal set; }

    public bool IsAllDay { get; internal set; }


    public static OneOf<CalendarEvent, ArgumentException> Create(
        Guid calendarId,
        string title,
        string? description,
        EventDate startEndDate,
        bool isAllDayEvent,
        string? timeZone,
        Guid? scheduleId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return new ArgumentException("Title is required", nameof(title));
        }

        var calendarEvent = new CalendarEvent
        {
            Id = Guid.NewGuid(),
            CalendarId = calendarId,
            Title = title,
            Description = description,
            StartDate = startEndDate.StartDate,
            EndDate = startEndDate.EndDate,
            IsAllDay = isAllDayEvent,
            TimeZone = timeZone,
            ScheduleId = scheduleId
        };

        return calendarEvent;
    }

    public void Reschedule([NotNull] EventDate startEndDate, bool isAllDay, string? timeZone)
    {
        StartDate = startEndDate.StartDate;
        EndDate = startEndDate.EndDate;
        IsAllDay = isAllDay;
        TimeZone = timeZone;
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
