using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Availabilities;
using Refit;

namespace McpSandbox.Mcp.Api;

public interface IAvailabilitiesApi
{
    [Get("/api/availabilities/{id}")]
    Task<IApiResponse<AvailabilityDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/availabilities")]
    Task<IApiResponse<PagedResult<AvailabilityDto>>> SearchAsync(
        [Query] Guid? userId = null,
        [Query] Guid? officeId = null,
        [Query] bool? isActive = null,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);

    [Post("/api/availabilities")]
    Task<IApiResponse<AvailabilityDto>> CreateAsync([Body] CreateAvailabilityRequest request, CancellationToken cancellationToken = default);

    [Put("/api/availabilities/{id}")]
    Task<IApiResponse<AvailabilityDto>> UpdateAsync(Guid id, [Body] UpdateAvailabilityRequest request, CancellationToken cancellationToken = default);

    [Delete("/api/availabilities/{id}")]
    Task<IApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
