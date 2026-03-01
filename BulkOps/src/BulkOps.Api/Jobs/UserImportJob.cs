using BulkOps.Api.Repositories;
using BulkOps.Api.Services;

namespace BulkOps.Api.Jobs;

public class UserImportJob(
    IFakeUserGenerator generator,
    IUserBulkRepository repository,
    ILogger<UserImportJob> logger)
{
    public async Task ImportUsersAsync(int count, CancellationToken cancellationToken)
    {
        var actualCount = count <= 0 ? 5000 : count;
        logger.LogInformation("Starting background import for {Count} users.", actualCount);

        var batch = generator.Generate(actualCount);
        var inserted = await repository.BulkImportAsync(batch, cancellationToken);

        logger.LogInformation("Background import completed. Inserted {InsertedCount} users.", inserted);
    }
}
