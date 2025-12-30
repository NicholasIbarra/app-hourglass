using Scheduler.Domain.Entities.Schedules;

namespace Scheduler.Application.Schedules.Contracts;

public record EditSeriesDto
{
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsAllDayEvent { get; init; }
    public string TimeZone { get; init; } = "";
    public RecurrenceFrequency RecurrenceFrequency { get; init; }
    public int RecurrenceInterval { get; init; }
    public DayOfTheWeek? RecurrenceDayOfWeek { get; init; }
    public int? RecurrenceDayOfMonth { get; init; }
    public int? RecurrenceMonth { get; init; }
    public int? RecurrenceOccurrenceCount { get; init; }
    public DateTime? RecurrenceEndDate { get; init; }
}
