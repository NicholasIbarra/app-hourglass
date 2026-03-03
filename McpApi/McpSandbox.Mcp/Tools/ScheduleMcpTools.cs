using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Schedule;
using McpSandbox.Mcp.Api;
using ModelContextProtocol.Server;
using System.ComponentModel;
using static McpSandbox.Mcp.Api.ApiResponseExtensions;

namespace McpSandbox.Mcp.Tools;

[McpServerToolType]
public sealed class ScheduleMcpTools(IScheduleApi scheduleApi)
{
    [McpServerTool, Description("Get available users for a given date range and office, excluding those with unavailabilities or existing shift request assignments.")]
    public Task<IReadOnlyList<ScheduleAvailabilityDto>> Schedule_GetAvailability(
        [Description("The start date of the range (yyyy-MM-dd).")] string startDate,
        [Description("The end date of the range (yyyy-MM-dd).")] string endDate,
        [Description("The office ID to check availability for.")] string officeId)
        => scheduleApi.GetAvailabilityAsync(
            ParseDate(startDate),
            ParseDate(endDate),
            Parse(officeId)).UnwrapAsync();

    [McpServerTool, Description("Create a shift request assigning a user to an office for a given time range.")]
    public Task<ShiftRequestDto> ShiftRequests_Create(
        [Description("The ID of the user to assign.")] string userId,
        [Description("The ID of the office.")] string officeId,
        [Description("The shift start date and time (ISO 8601, e.g. 2025-01-15T09:00:00Z).")] string startAt,
        [Description("The shift end date and time (ISO 8601, e.g. 2025-01-15T17:00:00Z).")] string endAt,
        [Description("Optional reason for the shift request.")] string? reason = null,
        [Description("Optional notes.")] string? notes = null)
        => scheduleApi.CreateShiftRequestAsync(new CreateShiftRequestRequest(
            Parse(userId),
            Parse(officeId),
            ParseDateTimeOffset(startAt),
            ParseDateTimeOffset(endAt),
            reason,
            notes)).UnwrapAsync();

    [McpServerTool, Description("Get a shift request by ID.")]
    public Task<ShiftRequestDto> ShiftRequests_Get(
        [Description("The shift request ID.")] string id)
        => scheduleApi.GetShiftRequestByIdAsync(Parse(id)).UnwrapAsync();

    [McpServerTool, Description("List shift requests with optional filters.")]
    public Task<PagedResult<ShiftRequestDto>> ShiftRequests_List(
        [Description("Filter by user ID.")] string? userId = null,
        [Description("Filter by office ID.")] string? officeId = null,
        [Description("Filter by status (Pending, Approved, Rejected, Cancelled).")] string? status = null,
        [Description("Return shifts that overlap on or after this date (yyyy-MM-dd).")] string? startDate = null,
        [Description("Return shifts that overlap on or before this date (yyyy-MM-dd).")] string? endDate = null,
        int page = 1,
        int pageSize = 20)
        => scheduleApi.SearchShiftRequestsAsync(
            ParseOptional(userId),
            ParseOptional(officeId),
            ParseOptionalStatus(status),
            string.IsNullOrEmpty(startDate) ? null : ParseDate(startDate),
            string.IsNullOrEmpty(endDate) ? null : ParseDate(endDate),
            page,
            pageSize).UnwrapAsync();

    [McpServerTool, Description("Approve a pending shift request.")]
    public Task<ShiftRequestDto> ShiftRequests_Approve(
        [Description("The shift request ID.")] string id)
        => scheduleApi.UpdateShiftRequestStatusAsync(
            Parse(id),
            new UpdateShiftRequestStatusRequest(ShiftRequestStatus.Approved, null)).UnwrapAsync();

    [McpServerTool, Description("Reject a shift request.")]
    public Task<ShiftRequestDto> ShiftRequests_Reject(
        [Description("The shift request ID.")] string id,
        [Description("Optional notes explaining the rejection.")] string? notes = null)
        => scheduleApi.UpdateShiftRequestStatusAsync(
            Parse(id),
            new UpdateShiftRequestStatusRequest(ShiftRequestStatus.Rejected, notes)).UnwrapAsync();

    [McpServerTool, Description("Cancel a shift request.")]
    public Task<ShiftRequestDto> ShiftRequests_Cancel(
        [Description("The shift request ID.")] string id,
        [Description("Optional notes explaining the cancellation.")] string? notes = null)
        => scheduleApi.CancelShiftRequestAsync(
            Parse(id),
            new CancelShiftRequestRequest(notes)).UnwrapAsync();

    [McpServerTool, Description("Permanently delete a shift request.")]
    public async Task ShiftRequests_Delete(
        [Description("The shift request ID.")] string id)
        => (await scheduleApi.DeleteShiftRequestAsync(Parse(id))).EnsureSuccess();

    private static Guid Parse(string id)
        => Guid.TryParse(id, out var g)
            ? g
            : throw new ArgumentException("Invalid UUID", nameof(id));

    private static Guid? ParseOptional(string? id)
        => string.IsNullOrWhiteSpace(id) ? null
            : Guid.TryParse(id, out var g) ? g
            : throw new ArgumentException("Invalid UUID", nameof(id));

    private static DateOnly ParseDate(string date)
        => DateOnly.TryParse(date, out var d)
            ? d
            : throw new ArgumentException("Invalid date, expected yyyy-MM-dd.", nameof(date));

    private static DateTimeOffset ParseDateTimeOffset(string dt)
        => DateTimeOffset.TryParse(dt, out var d)
            ? d
            : throw new ArgumentException("Invalid date/time, expected ISO 8601.", nameof(dt));

    private static ShiftRequestStatus? ParseOptionalStatus(string? status)
        => string.IsNullOrWhiteSpace(status) ? null
            : Enum.TryParse<ShiftRequestStatus>(status, ignoreCase: true, out var s) ? s
            : throw new ArgumentException($"Invalid status '{status}'. Expected: Pending, Approved, Rejected, or Cancelled.", nameof(status));
}

