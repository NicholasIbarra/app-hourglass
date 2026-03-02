namespace McpSandbox.Api.Contracts.Unavailabilities;

public sealed record CreateUnavailabilityRequest(
    Guid UserId,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsAllDay,
    string? Reason,
    string? Notes,
    bool IsActive);

public sealed record UpdateUnavailabilityRequest(
    Guid UserId,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsAllDay,
    string? Reason,
    string? Notes,
    bool IsActive);

public sealed record UnavailabilityDto(
    Guid Id,
    Guid UserId,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsAllDay,
    string? Reason,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
