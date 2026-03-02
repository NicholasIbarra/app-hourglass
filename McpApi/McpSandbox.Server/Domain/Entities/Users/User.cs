using McpSandbox.Api.Contracts.Users;
using McpSandbox.Server.Domain.Entities.Offices;

namespace McpSandbox.Server.Domain.Entities.Users;

public class User : BaseEntity
{
    public required string Name { get; set; }

    public string? Email { get; set; }

    public UserType Type { get; set; }

    public bool IsActive { get; set; } = true;

    public string? PhoneNumber { get; set; }

    public string? TimeZone { get; set; }

    public string? Locale { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public virtual ICollection<Office> Offices { get; set; } = [];
}
