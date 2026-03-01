using BulkOps.Api.Services;

namespace BulkOps.Api.Repositories;

public interface IUserBulkRepository
{
    Task<int> BulkImportAsync(GeneratedUserBatch batch, CancellationToken cancellationToken = default);
}
