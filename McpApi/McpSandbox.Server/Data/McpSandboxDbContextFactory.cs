using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace McpSandbox.Server.Data;

/// <summary>
/// Design-time factory for creating McpSandboxDbContext instances.
/// This enables EF Core tools (migrations, scaffolding) to work from Package Manager Console.
/// </summary>
public sealed class McpSandboxDbContextFactory : IDesignTimeDbContextFactory<McpSandboxDbContext>
{
    public McpSandboxDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<McpSandboxDbContext>();

        // Use a design-time connection string for migrations.
        // This can be:
        // 1. LocalDB (default for Visual Studio)
        // 2. SQL Server Express
        // 3. A connection string from environment variable
        // 4. A connection string passed via args

        var connectionString = Environment.GetEnvironmentVariable("DESIGN_TIME_CONNECTION_STRING")
            ?? (args.Length > 0 ? args[0] : null)
            ?? "Server=(localdb)\\mssqllocaldb;Database=McpSandbox;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);

        return new McpSandboxDbContext(optionsBuilder.Options);
    }
}
