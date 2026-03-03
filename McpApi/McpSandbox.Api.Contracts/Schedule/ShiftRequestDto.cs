namespace McpSandbox.Api.Contracts.Schedule;

public sealed record ShiftRequestDto(
    Guid Id,
    Guid UserId,
    Guid OfficeId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    ShiftRequestStatus Status,
    DateTimeOffset RequestedAt,
    string? Reason,
    string? Notes);
