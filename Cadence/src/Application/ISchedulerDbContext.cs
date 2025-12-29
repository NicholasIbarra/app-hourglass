using Microsoft.EntityFrameworkCore;
using Scheduler.Domain.Entities.Calendars;
using Scheduler.Domain.Entities.Schedules;

namespace Scheduler.Application;

public interface ISchedulerDbContext
{
    DbSet<Calendar> Calendars { get; }

    DbSet<Schedule> Schedules { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
