using Microsoft.Extensions.Hosting;
using Scheduler.Infrastructure.Persistence;

namespace Scheduler.Infrastructure;

public static class Startup
{
    public static IHostApplicationBuilder AddSchedulerInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddDatabase(builder.Configuration);

        return builder;
    }
}
