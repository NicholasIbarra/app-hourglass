using Scheduler.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddSchedulerInfrastructure();

var app = builder.Build();

app.UseServiceDefaults();

app.Run();