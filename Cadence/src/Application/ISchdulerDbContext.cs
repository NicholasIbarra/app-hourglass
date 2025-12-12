using Microsoft.EntityFrameworkCore;
using Scheduler.Domain.Entities.Calendars;

namespace Scheduler.Application;

public interface ISchdulerDbContext
{
    DbSet<Calendar> Calendars { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
