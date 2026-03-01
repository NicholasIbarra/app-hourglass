namespace BulkOps.Api.Entities;

public class User
{
    public long Id { get; set; }

    public required string ExternalId { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public ICollection<Office> Offices { get; set; } = new List<Office>();

    public ICollection<UserOffice> UserOffices { get; set; } = new List<UserOffice>();
}
