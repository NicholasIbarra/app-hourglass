namespace BulkOps.Api.Services;

public interface IFakeUserGenerator
{
    GeneratedUserBatch Generate(int userCount);
}
