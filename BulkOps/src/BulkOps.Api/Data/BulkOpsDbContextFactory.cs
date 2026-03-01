using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BulkOps.Api.Data;

public class BulkOpsDbContextFactory : IDesignTimeDbContextFactory<BulkOpsDbContext>
{
    public BulkOpsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BulkOpsDbContext>();
        
        // Use a default connection string for migrations
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BulkOpsDb;Trusted_Connection=True;MultipleActiveResultSets=true");
        
        return new BulkOpsDbContext(optionsBuilder.Options);
    }
}
