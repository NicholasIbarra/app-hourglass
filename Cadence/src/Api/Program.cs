using Scheduler.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddSchedulerInfrastructure();

var app = builder.Build();

app.UseServiceDefaults();

await app.ApplyMigrationsAsync();

app.Run();