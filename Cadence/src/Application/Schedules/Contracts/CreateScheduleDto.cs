using Scheduler.Domain.Entities.Schedules;

namespace Scheduler.Application.Schedules.Contracts;

public record CreateScheduleDto
{
    public Guid CalendarId { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsAllDayEvent { get; init; }
    public string? TimeZone { get; init; }
    public RecurrenceFrequency RecurrenceFrequency { get; init; }
    public int RecurrenceInterval { get; init; }
    public int? RecurrenceDayOfMonth { get; init; }
    public int? RecurrenceMonth { get; init; }
    public int? RecurrenceOccurrenceCount { get; init; }
    public DateTime? RecurrenceEndDate { get; init; }

    // Weekly selection booleans for API consumers
    public bool IsSunday { get; init; }
    public bool IsMonday { get; init; }
    public bool IsTuesday { get; init; }
    public bool IsWednesday { get; init; }
    public bool IsThursday { get; init; }
    public bool IsFriday { get; init; }
    public bool IsSaturday { get; init; }
}
