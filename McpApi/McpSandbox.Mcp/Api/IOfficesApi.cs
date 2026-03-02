using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Offices;
using Refit;

namespace McpSandbox.Mcp.Api;

public interface IOfficesApi
{
    [Get("/api/offices/{id}")]
    Task<OfficeDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/offices")]
    Task<PagedResult<OfficeDto>> SearchAsync(
        [Query] string? search = null,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);
}
