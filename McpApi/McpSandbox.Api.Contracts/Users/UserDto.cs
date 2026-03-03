using System.ComponentModel;

namespace McpSandbox.Api.Contracts.Users;

public sealed record CreateUserRequest(
    [property: Description("The user's name.")] string Name,
    [property: Description("The user's email address.")] string? Email,
    [property: Description("The user's type (Staff, Provider, or NonScheduling).")] UserType Type,
    [property: Description("Whether the user is active.")] bool IsActive,
    [property: Description("The user's phone number.")] string? PhoneNumber,
    [property: Description("The user's time zone (e.g. America/New_York).")] string? TimeZone,
    [property: Description("The user's locale (e.g. en-US).")] string? Locale,
    [property: Description("URL to the user's avatar image.")] string? AvatarUrl,
    [property: Description("The user's last login timestamp.")] DateTimeOffset? LastLoginAt,
    [property: Description("List of office IDs to assign the user to.")] IReadOnlyList<Guid>? OfficeIds);

public sealed record UpdateUserRequest(
    [property: Description("The user's name.")] string Name,
    [property: Description("The user's email address.")] string? Email,
    [property: Description("The user's type (Staff, Provider, or NonScheduling).")] UserType Type,
    [property: Description("Whether the user is active.")] bool IsActive,
    [property: Description("The user's phone number.")] string? PhoneNumber,
    [property: Description("The user's time zone (e.g. America/New_York).")] string? TimeZone,
    [property: Description("The user's locale (e.g. en-US).")] string? Locale,
    [property: Description("URL to the user's avatar image.")] string? AvatarUrl,
    [property: Description("The user's last login timestamp.")] DateTimeOffset? LastLoginAt,
    [property: Description("List of office IDs to assign the user to.")] IReadOnlyList<Guid>? OfficeIds);

public sealed class PatchUserRequest
{
    [Description("The user's name.")]
    public string? Name { get; set; }

    [Description("The user's email address.")]
    public string? Email { get; set; }

    [Description("The user's type (Staff, Provider, or NonScheduling).")]
    public UserType? Type { get; set; }

    [Description("Whether the user is active.")]
    public bool? IsActive { get; set; }

    [Description("The user's phone number.")]
    public string? PhoneNumber { get; set; }

    [Description("The user's time zone (e.g. America/New_York).")]
    public string? TimeZone { get; set; }

    [Description("The user's locale (e.g. en-US).")]
    public string? Locale { get; set; }

    [Description("URL to the user's avatar image.")]
    public string? AvatarUrl { get; set; }

    [Description("The user's last login timestamp.")]
    public DateTimeOffset? LastLoginAt { get; set; }

    [Description("List of office IDs to assign the user to.")]
    public IReadOnlyList<Guid>? OfficeIds { get; set; }
}

public sealed record UserOfficeDto(
    Guid Id,
    string Name);

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
    IReadOnlyList<UserOfficeDto> Offices);
