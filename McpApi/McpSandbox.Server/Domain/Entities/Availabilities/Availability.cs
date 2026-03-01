using McpSandbox.Server.Domain.Entities.Availabilities.ValueObjects;
using McpSandbox.Server.Domain.Entities.Offices;
using McpSandbox.Server.Domain.Entities.Users;

namespace McpSandbox.Server.Domain.Entities.Availabilities;

public class Availability : BaseEntity
{
    public Guid UserId { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;

    public AvailabilityDayTimeRange? Sunday { get; set; }

    public AvailabilityDayTimeRange? Monday { get; set; }

    public AvailabilityDayTimeRange? Tuesday { get; set; }

    public AvailabilityDayTimeRange? Wednesday { get; set; }

    public AvailabilityDayTimeRange? Thursday { get; set; }

    public AvailabilityDayTimeRange? Friday { get; set; }

    public AvailabilityDayTimeRange? Saturday { get; set; }

    public virtual List<Office> Offices { get; set; } = new();

    public virtual User User { get; set; }
}
