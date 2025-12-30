using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Domain.Entities.Schedules;

/// <summary>
/// Represents the type of exception that can occur in a schedule.
/// </summary>
public enum ScheduleExceptionType
{
    /// <summary>
    /// Indicates that the schedule occurrence was cancelled and will not be executed.
    /// </summary>
    Skipped = 1,

    /// <summary>
    /// Indicates that the schedule occurrence was rescheduled to a different time or date.
    /// </summary>
    Rescheduled = 2,

    /// <summary>
    /// The schedule occurrence was materialized and has a corresponding event created for it.
    /// </summary>
    Materialized = 3,
}
