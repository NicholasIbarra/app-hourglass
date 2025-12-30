using Scheduler.Domain.Entities.Schedules;

namespace Scheduler.Application.Schedules.Extensions;

public static class WeekdayFlagsExtensions
{
    public static DayOfTheWeek? ToDayOfTheWeekFlags(
        bool isSunday,
        bool isMonday,
        bool isTuesday,
        bool isWednesday,
        bool isThursday,
        bool isFriday,
        bool isSaturday,
        RecurrenceFrequency frequency)
    {
        if (frequency != RecurrenceFrequency.Weekly)
            return null;

        DayOfTheWeek flags = DayOfTheWeek.None;
        if (isSunday) flags |= DayOfTheWeek.Sunday;
        if (isMonday) flags |= DayOfTheWeek.Monday;
        if (isTuesday) flags |= DayOfTheWeek.Tuesday;
        if (isWednesday) flags |= DayOfTheWeek.Wednesday;
        if (isThursday) flags |= DayOfTheWeek.Thursday;
        if (isFriday) flags |= DayOfTheWeek.Friday;
        if (isSaturday) flags |= DayOfTheWeek.Saturday;
        return flags;
    }
}
