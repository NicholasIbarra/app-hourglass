namespace Scheduler.Domain.Entities.Schedules;

public enum ScheduledEventInstanceType
{
    Pseudo = 1,     // Generated from schedule
    Persisted = 2   // Comes from exception override
}
