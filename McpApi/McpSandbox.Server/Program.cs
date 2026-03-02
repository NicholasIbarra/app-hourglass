using ModelContextProtocol.Server;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("mcpdb")
    ?? builder.Configuration.GetConnectionString("McpSandbox")
    ?? builder.Configuration["ConnectionStrings__mcpdb"]
    ?? builder.Configuration["ConnectionStrings__McpSandbox"];

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<McpSandbox.Server.Data.McpSandboxDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<McpSandbox.Server.Data.McpSandboxDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

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

app.MapDefaultEndpoints();
app.UseFileServer();

app.MapMcp("/mcp");

app.Run();
