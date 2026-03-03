using McpSandbox.Api.Contracts.Schedule;
using McpSandbox.Server.Domain.Entities.Offices;
using McpSandbox.Server.Domain.Entities.Users;

namespace McpSandbox.Server.Domain.Entities.ShiftRequests;

public class ShiftRequest : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid OfficeId { get; set; }

    public DateTimeOffset StartAt { get; set; }

    public DateTimeOffset EndAt { get; set; }

    public ShiftRequestStatus Status { get; set; } = ShiftRequestStatus.Pending;

    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Office Office { get; set; } = null!;
}
