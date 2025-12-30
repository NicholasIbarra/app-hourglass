using Scheduler.Domain.Entities.Schedules;

namespace Scheduler.Domain.Managers;

public interface IScheduledEventResolver
{
    IReadOnlyList<ScheduledEventInstance> Resolve(
        Schedule schedule,
        DateTime from,
        DateTime to);
}
