using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Schedule;
using Refit;

namespace McpSandbox.Mcp.Api;

public interface IScheduleApi
{
    [Get("/api/schedule/availability")]
    Task<IApiResponse<IReadOnlyList<ScheduleAvailabilityDto>>> GetAvailabilityAsync(
        [Query] DateOnly startDate,
        [Query] DateOnly endDate,
        [Query] Guid officeId,
        CancellationToken cancellationToken = default);

    [Post("/api/schedule/shift-request")]
    Task<IApiResponse<ShiftRequestDto>> CreateShiftRequestAsync(
        [Body] CreateShiftRequestRequest request,
        CancellationToken cancellationToken = default);

    [Get("/api/schedule/shift-request/{id}")]
    Task<IApiResponse<ShiftRequestDto>> GetShiftRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/schedule/shift-request")]
    Task<IApiResponse<PagedResult<ShiftRequestDto>>> SearchShiftRequestsAsync(
        [Query] Guid? userId = null,
        [Query] Guid? officeId = null,
        [Query] ShiftRequestStatus? status = null,
        [Query] DateOnly? startDate = null,
        [Query] DateOnly? endDate = null,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);

    [Put("/api/schedule/shift-request/{id}/status")]
    Task<IApiResponse<ShiftRequestDto>> UpdateShiftRequestStatusAsync(
        Guid id,
        [Body] UpdateShiftRequestStatusRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/schedule/shift-request/{id}/cancel")]
    Task<IApiResponse<ShiftRequestDto>> CancelShiftRequestAsync(
        Guid id,
        [Body] CancelShiftRequestRequest request,
        CancellationToken cancellationToken = default);

    [Delete("/api/schedule/shift-request/{id}")]
    Task<IApiResponse> DeleteShiftRequestAsync(Guid id, CancellationToken cancellationToken = default);
}

