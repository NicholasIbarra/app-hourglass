using System.ComponentModel;

namespace McpSandbox.Api.Contracts.Offices;

public sealed record CreateOfficeRequest(
    [property: Description("The office name.")] string Name,
    [property: Description("A short code for the office.")] string? Code,
    [property: Description("Whether the office is active.")] bool IsActive,
    [property: Description("The office phone number.")] string? PhoneNumber,
    [property: Description("The office time zone (e.g. America/New_York).")] string? TimeZone,
    [property: Description("Notes about the office.")] string? Notes,
    [property: Description("Street address line 1.")] string? AddressLine1,
    [property: Description("Street address line 2.")] string? AddressLine2,
    [property: Description("City.")] string? City,
    [property: Description("State or province.")] string? StateOrProvince,
    [property: Description("Postal / ZIP code.")] string? PostalCode,
    [property: Description("Country.")] string? Country);

public sealed record UpdateOfficeRequest(
    [property: Description("The office name.")] string Name,
    [property: Description("A short code for the office.")] string? Code,
    [property: Description("Whether the office is active.")] bool IsActive,
    [property: Description("The office phone number.")] string? PhoneNumber,
    [property: Description("The office time zone (e.g. America/New_York).")] string? TimeZone,
    [property: Description("Notes about the office.")] string? Notes,
    [property: Description("Street address line 1.")] string? AddressLine1,
    [property: Description("Street address line 2.")] string? AddressLine2,
    [property: Description("City.")] string? City,
    [property: Description("State or province.")] string? StateOrProvince,
    [property: Description("Postal / ZIP code.")] string? PostalCode,
    [property: Description("Country.")] string? Country);

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
