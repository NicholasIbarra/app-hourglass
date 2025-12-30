using System;
using System.Collections.Generic;
using System.Text;

namespace Scheduler.Domain.Entities.Schedules;

public sealed class ScheduledEventInstance
{
    public DateTime OccursAt { get; }
    public ScheduledEventInstanceType Type { get; }
    public Guid? EventId { get; }

    private ScheduledEventInstance(
        DateTime occursAt,
        ScheduledEventInstanceType type,
        Guid? eventId)
    {
        OccursAt = occursAt;
        Type = type;
        EventId = eventId;
    }

    public static ScheduledEventInstance Pseudo(DateTime occursAt)
        => new(occursAt, ScheduledEventInstanceType.Pseudo, null);

    public static ScheduledEventInstance Persisted(
        DateTime occursAt,
        Guid eventId)
        => new(occursAt, ScheduledEventInstanceType.Persisted, eventId);
}
