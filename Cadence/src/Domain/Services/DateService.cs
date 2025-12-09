using Scheduler.Domain.Entities.Schedules;
using System.Diagnostics.CodeAnalysis;

namespace Scheduler.Domain.Services;

public static class DateService
{
    public static DateTime GetNextOccurrence(DateTime currentDateTime, [NotNull] RecurrencePattern recurrencePattern)
    {
        if (recurrencePattern.Interval < 1)
            throw new ArgumentOutOfRangeException(nameof(recurrencePattern.Interval));

        return recurrencePattern.Frequency switch
        {
            RecurrenceFrequency.Daily => currentDateTime.AddDays(recurrencePattern.Interval),

            RecurrenceFrequency.Weekly => NextWeekly(currentDateTime, recurrencePattern),

            RecurrenceFrequency.Monthly => NextMonthly(currentDateTime, recurrencePattern),

            RecurrenceFrequency.Yearly => NextYearly(currentDateTime, recurrencePattern),

            _ => throw new ArgumentOutOfRangeException(nameof(recurrencePattern.Frequency))
        };
    }

    private static DateTime NextWeekly(DateTime currentDateTime, RecurrencePattern recurrencePattern)
    {
        if (!recurrencePattern.DayOfWeek.HasValue)
            throw new InvalidOperationException("DayOfWeek must be specified for weekly recurrence.");

        var flags = recurrencePattern.DayOfWeek.Value;
        if ((int)flags == 0)
            throw new InvalidOperationException("DayOfWeek flags cannot be empty.");

        var interval = recurrencePattern.Interval;
        if (interval < 1)
            throw new ArgumentOutOfRangeException(nameof(recurrencePattern.Interval), "Interval must be >= 1.");

        // 1) Baseline: earliest flagged day strictly after currentDateTime (preserve time-of-day)
        var baseline = FindNextFlaggedSameOrNextWeek(currentDateTime, flags);

        if (interval == 1)
            return baseline;

        // 2) Decide how to apply week-skipping
        bool baselineInSameWeek = StartOfWeek(baseline) == StartOfWeek(currentDateTime);
        bool singleDay = CountSetBits((int)flags) == 1;

        if (singleDay)
        {
            // Single flagged day: always skip (interval-1) weeks from the baseline
            return baseline.AddDays(7 * (interval - 1));
        }

        // Multiple flagged days:
        // - If we already found a match within the SAME week, take it (do NOT skip weeks).
        // - Otherwise (match is in a future week), skip (interval-1) additional weeks.
        return baselineInSameWeek
            ? baseline
            : baseline.AddDays(7 * (interval - 1));
    }

    private static DateTime FindNextFlaggedSameOrNextWeek(DateTime current, DayOfTheWeek flags)
    {
        // Scan strictly after "current" within the next 7 days.
        for (int i = 1; i <= 7; i++)
        {
            var candidate = current.Date.AddDays(i).Add(current.TimeOfDay);
            if (flags.HasFlag(ToDayOfWeekFlag(candidate.DayOfWeek)))
                return candidate;
        }
        // With non-empty flags this should never happen.
        throw new InvalidOperationException("No matching day found within the next 7 days.");
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        // Sunday-based week
        int diff = (int)date.DayOfWeek; // Sunday=0
        return date.Date.AddDays(-diff);
    }

    private static int CountSetBits(int value)
    {
        // Kernighan’s bit count
        int count = 0;
        while (value != 0) { value &= (value - 1); count++; }
        return count;
    }

    private static DateTime NextMonthly(DateTime currentDateTime, RecurrencePattern recurrencePattern)
    {
        if (recurrencePattern.DayOfMonth.HasValue)
        {
            return currentDateTime.AddMonths(recurrencePattern.Interval).AddDays(recurrencePattern.DayOfMonth.Value - currentDateTime.Day);
        }
        else
        {
            return currentDateTime.AddMonths(recurrencePattern.Interval);
        }
    }

    private static DateTime NextYearly(DateTime currentDateTime, RecurrencePattern recurrencePattern)
    {
        if (recurrencePattern.Month.HasValue && recurrencePattern.DayOfMonth.HasValue)
        {
            return new DateTime(
                currentDateTime.Year + recurrencePattern.Interval,
                recurrencePattern.Month.Value,
                recurrencePattern.DayOfMonth.Value,
                currentDateTime.Hour,
                currentDateTime.Minute,
                currentDateTime.Second);
        }
        else
        {
            return currentDateTime.AddYears(recurrencePattern.Interval);
        }
    }

    public static DayOfTheWeek ToDayOfWeekFlag(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Sunday => DayOfTheWeek.Sunday,
            DayOfWeek.Monday => DayOfTheWeek.Monday,
            DayOfWeek.Tuesday => DayOfTheWeek.Tuesday,
            DayOfWeek.Wednesday => DayOfTheWeek.Wednesday,
            DayOfWeek.Thursday => DayOfTheWeek.Thursday,
            DayOfWeek.Friday => DayOfTheWeek.Friday,
            DayOfWeek.Saturday => DayOfTheWeek.Saturday,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
        };
    }
}
