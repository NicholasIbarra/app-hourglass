using BulkOps.Api.Metrics;
using BulkOps.Api.Repositories;
using BulkOps.Api.Services;
using System.Diagnostics;

namespace BulkOps.Api.Jobs;

public class UserImportJob(
    IFakeUserGenerator generator,
    IUserBulkRepository repository,
    BulkImportMetrics metrics,
    ILogger<UserImportJob> logger)
{
    public async Task ImportUsersAsync(int count, CancellationToken cancellationToken)
    {
        var actualCount = count <= 0 ? 5000 : count;
        logger.LogInformation("Starting background import for {Count} users.", actualCount);

        var stopwatch = Stopwatch.StartNew();
        var batch = generator.Generate(actualCount);
        var inserted = await repository.BulkImportAsync(batch, cancellationToken);
        stopwatch.Stop();

        metrics.RecordImport(stopwatch.Elapsed.TotalSeconds, inserted);

        logger.LogInformation(
            "Background import completed. Inserted {InsertedCount} users in {ElapsedSeconds:F2}s.",
            inserted,
            stopwatch.Elapsed.TotalSeconds);
    }
}
