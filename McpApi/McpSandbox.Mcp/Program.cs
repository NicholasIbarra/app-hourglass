using McpSandbox.Mcp.Api;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddRefitClient<IUsersApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://server"));

builder.Services
    .AddRefitClient<IOfficesApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://server"));

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapDefaultEndpoints();

app.MapControllers();
app.MapMcp("/mcp");

app.Run();
