namespace BulkOps.Api.Entities;

public class Office
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string City { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();

    public ICollection<UserOffice> UserOffices { get; set; } = new List<UserOffice>();
}
