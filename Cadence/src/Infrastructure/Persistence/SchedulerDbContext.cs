using Microsoft.EntityFrameworkCore;
using Scheduler.Application;
using Scheduler.Domain.Entities.Calendars;
using System.Reflection;

namespace Scheduler.Infrastructure.Persistence;

public class SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) : DbContext(options), ISchedulerDbContext
{
    public DbSet<Calendar> Calendars => Set<Calendar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
