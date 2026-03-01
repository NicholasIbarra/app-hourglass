namespace BulkOps.Api.Entities;

public class UserOffice
{
    public long UserId { get; set; }

    public required User User { get; set; }

    public int OfficeId { get; set; }

    public required Office Office { get; set; }

    public DateTime AssignedAtUtc { get; set; }
}
