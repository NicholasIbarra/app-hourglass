using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace McpSandbox.Mcp.Data;

/// <summary>
/// Design-time factory for creating ChatDbContext instances.
/// This enables EF Core tools (migrations, scaffolding) to work from Package Manager Console.
/// </summary>
public sealed class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
{
    public ChatDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("DESIGN_TIME_CONNECTION_STRING")
            ?? (args.Length > 0 ? args[0] : null)
            ?? "Server=(localdb)\\mssqllocaldb;Database=McpSandboxChat;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);

        return new ChatDbContext(optionsBuilder.Options);
    }
}
