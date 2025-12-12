using Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scheduler.Application;
using Scheduler.Infrastructure.Persistence;
using Shared.EntityFramework;
using Wolverine;

namespace Scheduler.Infrastructure;

public static class Startup
{
    public static IHostApplicationBuilder AddSchedulerInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services
            .AddScoped<ISchdulerDbContext, SchedulerDbContext>()
            .AddEntityFramework<SchedulerDbContext>(builder.Configuration.GetConnectionString("TestDb")!);

        return builder;
    }
}

public static class WolverineStartup
{
    public static IHostApplicationBuilder AddWolverineBus(this IHostApplicationBuilder builder)
    {
        builder.Services.AddWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(SchedulerApplication).Assembly);
            opts.Policies.AutoApplyTransactions();
        });
        return builder;
    }
}
