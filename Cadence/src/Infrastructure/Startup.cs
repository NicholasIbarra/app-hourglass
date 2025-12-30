using Application;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scheduler.Application;
using Scheduler.Domain.Managers;
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
            .AddEntityFramework<SchedulerDbContext>(builder.Configuration.GetConnectionString("CadenceDbDev")!);

        // Register MediatR handlers from Application assembly
        builder.Services.AddValidatorsFromAssembly(typeof(SchedulerApplication).Assembly);
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SchedulerApplication).Assembly));
        builder.Services.AddScoped<IScheduledEventResolver, ScheduledEventResolver>();

        return builder;
    }

    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() == false)
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SchedulerDbContext>();

        await db.Database.MigrateAsync();
    }

}
