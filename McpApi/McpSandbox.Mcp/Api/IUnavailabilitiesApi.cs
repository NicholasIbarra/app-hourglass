using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Unavailabilities;
using Refit;

namespace McpSandbox.Mcp.Api;

public interface IUnavailabilitiesApi
{
    [Get("/api/unavailabilities/{id}")]
    Task<IApiResponse<UnavailabilityDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/unavailabilities")]
    Task<IApiResponse<PagedResult<UnavailabilityDto>>> SearchAsync(
        [Query] Guid? userId = null,
        [Query] string? search = null,
        [Query] DateTime? from = null,
        [Query] DateTime? to = null,
        [Query] bool? isActive = null,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);

    [Post("/api/unavailabilities")]
    Task<IApiResponse<UnavailabilityDto>> CreateAsync([Body] CreateUnavailabilityRequest request, CancellationToken cancellationToken = default);

    [Put("/api/unavailabilities/{id}")]
    Task<IApiResponse<UnavailabilityDto>> UpdateAsync(Guid id, [Body] UpdateUnavailabilityRequest request, CancellationToken cancellationToken = default);

    [Delete("/api/unavailabilities/{id}")]
    Task<IApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
