using System.ComponentModel;

namespace McpSandbox.Api.Contracts.Schedule;

public sealed record UpdateShiftRequestStatusRequest(
    [property: Description("The new status for the shift request.")] ShiftRequestStatus Status,
    [property: Description("Optional notes to attach to the status update.")] string? Notes);
