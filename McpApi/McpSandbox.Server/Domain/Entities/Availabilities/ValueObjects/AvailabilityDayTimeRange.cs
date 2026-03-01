namespace McpSandbox.Server.Domain.Entities.Availabilities.ValueObjects;

public readonly record struct AvailabilityDayTimeRange
{
    public AvailabilityDayTimeRange(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("EndTime must be after StartTime.", nameof(endTime));
        }

        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    public DayOfWeek DayOfWeek { get; }

    public TimeOnly StartTime { get; }

    public TimeOnly EndTime { get; }
}
