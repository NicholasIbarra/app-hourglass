using McpSandbox.Server.Domain.Entities.Users;

namespace McpSandbox.Server.Domain.Entities.Availabilities;

public class Unavailability : BaseEntity
{
    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsAllDay { get; set; }

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}