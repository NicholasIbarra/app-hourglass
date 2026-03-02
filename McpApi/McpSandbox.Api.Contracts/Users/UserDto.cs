namespace McpSandbox.Api.Contracts.Users;

public sealed record CreateUserRequest(
    string Name,
    string? Email,
    UserType Type,
    bool IsActive,
    string? PhoneNumber,
    string? TimeZone,
    string? Locale,
    string? AvatarUrl,
    DateTimeOffset? LastLoginAt,
    IReadOnlyList<Guid>? OfficeIds);

public sealed record UpdateUserRequest(
    string Name,
    string? Email,
    UserType Type,
    bool IsActive,
    string? PhoneNumber,
    string? TimeZone,
    string? Locale,
    string? AvatarUrl,
    DateTimeOffset? LastLoginAt,
    IReadOnlyList<Guid>? OfficeIds);

public sealed record UserDto(
    Guid Id,
    string Name,
    string? Email,
    UserType Type,
    bool IsActive,
    string? PhoneNumber,
    string? TimeZone,
    string? Locale,
    string? AvatarUrl,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<Guid> OfficeIds);
