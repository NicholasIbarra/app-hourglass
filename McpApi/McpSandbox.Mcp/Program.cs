using System.ClientModel;
using Azure.AI.OpenAI;
using McpSandbox.Mcp.Api;
using McpSandbox.Mcp.Data;
using McpSandbox.Mcp.Services.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// ChatDbContext
var chatConnectionString = builder.Configuration.GetConnectionString("chatdb");

if (!string.IsNullOrWhiteSpace(chatConnectionString))
{
    builder.Services.AddDbContext<ChatDbContext>(options =>
        options.UseSqlServer(chatConnectionString));
}

// Azure OpenAI
builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection(AzureOpenAIOptions.SectionName));

builder.Services.AddSingleton(sp =>
{
    var opts = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
    return new AzureOpenAIClient(
        new Uri(opts.Endpoint),
        new ApiKeyCredential(opts.ApiKey));
});

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<AzureOpenAIClient>();
    var opts = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
    return client.GetChatClient(opts.Model)
        .AsIChatClient()
        .AsBuilder()
        .UseFunctionInvocation()
        .Build();
});

builder.Services.AddSingleton<IMcpToolProvider, McpToolProvider>();

builder.Services.AddScoped<IChatAgentService, ChatAgentService>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var baseRefitSettings = new RefitSettings
{
    ContentSerializer = new NewtonsoftJsonContentSerializer()
};

builder.Services
    .AddRefitClient<IUsersApi>(baseRefitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://server"));

builder.Services
    .AddRefitClient<IOfficesApi>(baseRefitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://server"));

builder.Services
    .AddRefitClient<IAvailabilitiesApi>(baseRefitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://server"));

builder.Services
    .AddRefitClient<IUnavailabilitiesApi>(baseRefitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://server"));

builder.Services
    .AddRefitClient<IScheduleApi>(baseRefitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://server"));

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Run migrations on startup
if (!string.IsNullOrWhiteSpace(chatConnectionString))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    dbContext.Database.Migrate();
}

app.UseExceptionHandler();
app.UseCors();

// Configure the HTTP request pipeline.
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

app.UseAuthorization();
app.MapDefaultEndpoints();

app.MapControllers();
app.MapMcp("/mcp");

app.Run();
