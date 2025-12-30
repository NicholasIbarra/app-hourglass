namespace Scheduler.Application.Schedules.Contracts;

public record SkipOccurrenceDto
{
    public DateTime OccurrenceStartDate { get; init; }
}
