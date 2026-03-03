using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Offices;
using McpSandbox.Api.Contracts.Users;
using Microsoft.AspNetCore.JsonPatch;
using Refit;

namespace McpSandbox.Mcp.Api;

public interface IOfficesApi
{
    [Get("/api/offices/{id}")]
    Task<IApiResponse<OfficeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/offices")]
    Task<IApiResponse<PagedResult<OfficeDto>>> SearchAsync(
        [Query] string? search = null,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);

    [Post("/api/offices")]
    Task<IApiResponse<OfficeDto>> CreateAsync([Body] CreateOfficeRequest request, CancellationToken cancellationToken = default);

    [Put("/api/offices/{id}")]
    Task<IApiResponse<OfficeDto>> UpdateAsync(Guid id, [Body] UpdateOfficeRequest request, CancellationToken cancellationToken = default);

    [Delete("/api/offices/{id}")]
    Task<IApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
