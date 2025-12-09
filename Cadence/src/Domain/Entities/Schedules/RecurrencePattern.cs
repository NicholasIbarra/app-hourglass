namespace Scheduler.Domain.Entities.Schedules;

public record RecurrencePattern
{
    /// <summary>
    /// The frequency of the recurrence such as daily, weekly, monthly, or yearly.
    /// </summary>
    public RecurrenceFrequency Frequency { get; set; }

    /// <summary>
    /// The interval at which the event occurs. For example, if the event occurs every 2 days, the interval would be 2.
    /// </summary>
    public required int Interval { get; set; } = 1;

    /// <summary>
    /// The day of the week the event occurs. Only used if the frequency is weekly.
    /// </summary>

    public DayOfTheWeek? DayOfWeek { get; set; }

    /// <summary>
    /// The day of the month the event occurs. Only used if the frequency is monthly.
    /// </summary>

    public int? DayOfMonth { get; set; }

    /// <summary>
    /// The month the event occurs. Only used if the frequency is yearly.
    /// </summary>

    public int? Month { get; set; }

    /// <summary>
    /// The number of occurrences the event should repeat. If null, the event will repeat indefinitely.
    /// </summary>
    public int? OccurrenceCount { get; set; }
}

