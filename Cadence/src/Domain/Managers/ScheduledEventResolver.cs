using Scheduler.Domain.Entities.Schedules;
using Scheduler.Domain.Services;

namespace Scheduler.Domain.Managers;

public sealed class ScheduledEventResolver : IScheduledEventResolver
{
    public IReadOnlyList<ScheduledEventInstance> Resolve(
        Schedule schedule,
        DateTime from,
        DateTime to)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        if (to < from)
            throw new ArgumentException("End date must be after start date.");

        // Respect recurrence end date if present
        var effectiveTo = schedule.RecurrenceEndDate.HasValue
            ? (to <= schedule.RecurrenceEndDate.Value ? to : schedule.RecurrenceEndDate.Value)
            : to;

        // If the requested range starts after the recurrence ends, nothing to return
        if (schedule.RecurrenceEndDate.HasValue && from > schedule.RecurrenceEndDate.Value)
            return Array.Empty<ScheduledEventInstance>();

        var results = new List<ScheduledEventInstance>();

        var exceptionsByDate = schedule.Exceptions
            .ToDictionary(e => e.OriginalDate);

        var current = AlignStart(schedule, from);

        while (current <= effectiveTo)
        {
            if (exceptionsByDate.TryGetValue(current, out var exception))
            {
                switch (exception.ExceptionType)
                {
                    case ScheduleExceptionType.Skipped:
                        // Do nothing — event does not exist
                        break;

                    case ScheduleExceptionType.Rescheduled:
                    case ScheduleExceptionType.Materialized:
                        results.Add(
                            ScheduledEventInstance.Persisted(
                                current,
                                exception.EventId!.Value));
                        break;
                }
            }
            else
            {
                results.Add(
                    ScheduledEventInstance.Pseudo(current));
            }

            current = DateService.GetNextOccurrence(
                current,
                schedule.RecurrencePattern);
        }

        return results;
    }

    private static DateTime AlignStart(Schedule schedule, DateTime from)
    {
        var current = schedule.StartDate;

        while (current < from)
        {
            current = DateService.GetNextOccurrence(
                current,
                schedule.RecurrencePattern);
        }

        return current;
    }
}
