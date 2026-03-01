namespace McpSandbox.Server.Domain.Entities.Offices;

using McpSandbox.Server.Domain.Entities;

public sealed class Office : BaseEntity
{
    public required string Name { get; set; }

    public string? Code { get; set; }

    public bool IsActive { get; set; } = true;

    public string? PhoneNumber { get; set; }

    public string? TimeZone { get; set; }

    public string? Notes { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public string? StateOrProvince { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }
}
