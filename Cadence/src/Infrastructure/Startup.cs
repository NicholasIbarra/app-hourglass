using Application;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scheduler.Application;
using Scheduler.Infrastructure.Persistence;
using Shared.EntityFramework;

namespace Scheduler.Infrastructure;

public static class Startup
{
    public static IHostApplicationBuilder AddSchedulerInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services
            .AddScoped<ISchedulerDbContext, SchedulerDbContext>()
            .AddEntityFramework<SchedulerDbContext>(builder.Configuration.GetConnectionString("TestDb")!);

        // Register MediatR handlers from Application assembly
        builder.Services.AddValidatorsFromAssembly(typeof(SchedulerApplication).Assembly);
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SchedulerApplication).Assembly));

        return builder;
    }
}
