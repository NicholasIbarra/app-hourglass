using BulkOps.Api.Entities;

namespace BulkOps.Api.Services;

public class GeneratedUserBatch
{
    public required List<Office> Offices { get; init; }

    public required List<User> Users { get; init; }

    public required List<UserOffice> UserOffices { get; init; }
}
