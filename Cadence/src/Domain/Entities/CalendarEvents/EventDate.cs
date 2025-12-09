namespace Scheduler.Domain.Entities.CalendarEvents;

public record EventDate
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    public EventDate(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date.");
        }

        StartDate = startDate;
        EndDate = endDate;
    }
}

