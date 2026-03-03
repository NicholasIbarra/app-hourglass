using McpSandbox.Api.Contracts.Availabilities;

namespace McpSandbox.Api.Contracts.Schedule;

public sealed record ScheduleAvailabilityDto(
    Guid UserId,
    string UserName);
