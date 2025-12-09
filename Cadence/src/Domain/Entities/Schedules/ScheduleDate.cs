namespace Scheduler.Domain.Entities.Schedules;

public record ScheduleDate
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    public ScheduleDate(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date.");
        }

        StartDate = startDate;
        EndDate = endDate;
    }
}

