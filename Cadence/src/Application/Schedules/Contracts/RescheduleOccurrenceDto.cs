namespace Scheduler.Application.Schedules.Contracts;

public record RescheduleOccurrenceDto
{
    public DateTime OccurrenceStartDate { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsAllDayEvent { get; init; }
    public string? TimeZone { get; init; }
}
