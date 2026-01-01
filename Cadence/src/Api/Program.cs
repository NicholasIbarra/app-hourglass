using Scheduler.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddSchedulerInfrastructure();

var app = builder.Build();

app.UseServiceDefaults();

await app.ApplyMigrationsAsync();

app.Run();