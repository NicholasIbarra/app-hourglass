using Shared.Entities;

namespace Scheduler.Domain.Entities.Schedules;

public class ScheduleException : BaseEntity
{
    public Guid ScheduleId { get; private init; }

    public DateTime OriginalDate { get; private init; }

    public ScheduleExceptionType ExceptionType { get; private init; }

    public Guid? EventId { get; private init; }


    internal static ScheduleException Skip(Guid ScheduleId, DateTime originalDate)
    {
        var schduleException = new ScheduleException
        {
            ScheduleId = ScheduleId,
            OriginalDate = originalDate,
            ExceptionType = ScheduleExceptionType.Skipped
        };

        return schduleException;
    }

    internal static ScheduleException Reschedule(Guid scheduleId, DateTime originalDate, Guid rescheduledEventId)
    {
        var scheduleException = new ScheduleException
        {
            ScheduleId = scheduleId,
            OriginalDate = originalDate,
            ExceptionType = ScheduleExceptionType.Rescheduled,
            EventId = rescheduledEventId
        };
        return scheduleException;
    }

    internal static ScheduleException Materialize(Guid scheduleId, DateTime originalDate, Guid rescheduledEventId)
    {
        var scheduleException = new ScheduleException
        {
            ScheduleId = scheduleId,
            OriginalDate = originalDate,
            ExceptionType = ScheduleExceptionType.Materialized,
            EventId = rescheduledEventId
        };
        return scheduleException;
    }


}

