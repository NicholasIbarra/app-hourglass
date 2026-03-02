namespace McpSandbox.Api.Contracts.Offices;

public sealed record CreateOfficeRequest(
    string Name,
    string? Code,
    bool IsActive,
    string? PhoneNumber,
    string? TimeZone,
    string? Notes,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateOrProvince,
    string? PostalCode,
    string? Country);

public sealed record UpdateOfficeRequest(
    string Name,
    string? Code,
    bool IsActive,
    string? PhoneNumber,
    string? TimeZone,
    string? Notes,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateOrProvince,
    string? PostalCode,
    string? Country);

public sealed record OfficeDto(
    Guid Id,
    string Name,
    string? Code,
    bool IsActive,
    string? PhoneNumber,
    string? TimeZone,
    string? Notes,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? StateOrProvince,
    string? PostalCode,
    string? Country,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
