using Microsoft.EntityFrameworkCore;
using Scheduler.Application;
using Scheduler.Domain.Entities.CalendarEvents;
using Scheduler.Domain.Entities.Calendars;
using Scheduler.Domain.Entities.Schedules;
using System.Reflection;

namespace Scheduler.Infrastructure.Persistence;

public class SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) : DbContext(options), ISchedulerDbContext
{
    public DbSet<Calendar> Calendars => Set<Calendar>();

    public DbSet<Schedule> Schedules => Set<Schedule>();

    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
