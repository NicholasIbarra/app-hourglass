using BulkOps.Api.Data;
using BulkOps.Api.Entities;
using BulkOps.Api.Jobs;
using BulkOps.Api.Metrics;
using BulkOps.Api.Repositories;
using BulkOps.Api.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("bulkopsdb")
    ?? throw new InvalidOperationException("Connection string 'bulkopsdb' is not configured.");

builder.Services.AddDbContext<BulkOpsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<BulkOpsDbContext>();

builder.Services.AddScoped<IUserBulkRepository, UserBulkRepository>();
builder.Services.AddSingleton<IFakeUserGenerator, FakeUserGenerator>();
builder.Services.AddSingleton<BulkImportMetrics>();
builder.Services.AddScoped<UserImportJob>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddMeter(BulkImportMetrics.MeterName));

builder.Services.AddHangfire(configuration =>
{
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            SchemaName = "hangfire",
            PrepareSchemaIfNecessary = true,
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(10),
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        });
});

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseServiceDefaults();
app.MapOpenApi();
app.UseHangfireDashboard("/jobs");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
        options.RoutePrefix = "swagger";
    });
}

app.MapPost("/imports/users", (IBackgroundJobClient backgroundJobs, int? count) =>
{
    var importCount = count.GetValueOrDefault(5000);

    var jobId = backgroundJobs.Enqueue<UserImportJob>(job =>
        job.ImportUsersAsync(importCount, CancellationToken.None));

    return Results.Accepted($"/jobs/details/{jobId}", new
    {
        jobId,
        importCount,
        message = "Bulk user import has been queued."
    });
});

app.MapGet("/imports/users/demo-payload", (IFakeUserGenerator generator) =>
{
    var payload = generator.Generate(5000);
    return Results.Ok(new
    {
        users = payload.Users.Count,
        offices = payload.Offices.Count,
        assignments = payload.Users.Sum(u => u.UserOffices.Count)
    });
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BulkOpsDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var roleName in RoleCatalog.StandardRoles)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            continue;
        }

        var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"Failed to seed role '{roleName}': {errors}");
        }
    }
}

app.Run();
