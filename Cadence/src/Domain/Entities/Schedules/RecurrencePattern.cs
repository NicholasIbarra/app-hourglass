using OneOf;

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
    public int Interval { get; set; } = 1;

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

    internal RecurrencePattern() { }

    private RecurrencePattern(
        RecurrenceFrequency frequency,
        int interval,
        DayOfTheWeek? dayOfWeek,
        int? dayOfMonth,
        int? month,
        int? occurrenceCount)
    {
        Frequency = frequency;
        Interval = interval;
        DayOfWeek = dayOfWeek;
        DayOfMonth = dayOfMonth;
        Month = month;
        OccurrenceCount = occurrenceCount;
    }

    public static OneOf<RecurrencePattern, ArgumentException> Create(
        RecurrenceFrequency frequency,
        int interval = 1,
        DayOfTheWeek? dayOfWeek = null,
        int? dayOfMonth = null,
        int? month = null,
        int? occurrenceCount = null)
    {
        if (interval <= 0)
            return new ArgumentException("Interval must be greater than zero.");

        if (occurrenceCount <= 0)
            return new ArgumentException("OccurrenceCount must be greater than zero.");

        switch (frequency)
        {
            case RecurrenceFrequency.Daily:
                // No additional validation needed for daily frequency
                break;
            case RecurrenceFrequency.Weekly:
                if (dayOfWeek is null || dayOfWeek == DayOfTheWeek.None)
                    return new ArgumentException("DayOfWeek is required for weekly recurrence.");
                break;

            case RecurrenceFrequency.Monthly:
                if (dayOfMonth is null or < 1 or > 31)
                    return new ArgumentException("DayOfMonth must be between 1 and 31 for monthly recurrence.");
                break;

            case RecurrenceFrequency.Yearly:
                if (month is null or < 1 or > 12)
                    return new ArgumentException("Month must be between 1 and 12 for yearly recurrence.");
                break;
            default:
                return new ArgumentException("Invalid recurrence frequency.");
        }

        return new RecurrencePattern(
            frequency,
            interval,
            dayOfWeek,
            dayOfMonth,
            month,
            occurrenceCount);
    }

}

