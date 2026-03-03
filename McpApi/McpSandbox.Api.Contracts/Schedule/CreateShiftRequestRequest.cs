using System.ComponentModel;

namespace McpSandbox.Api.Contracts.Schedule;

public sealed record CreateShiftRequestRequest(
    [property: Description("The ID of the user to assign to the shift.")] Guid UserId,
    [property: Description("The ID of the office for the shift.")] Guid OfficeId,
    [property: Description("The shift start date and time (UTC).")] DateTimeOffset StartAt,
    [property: Description("The shift end date and time (UTC).")] DateTimeOffset EndAt,
    [property: Description("The reason for the shift request.")] string? Reason,
    [property: Description("Any additional notes.")] string? Notes);
