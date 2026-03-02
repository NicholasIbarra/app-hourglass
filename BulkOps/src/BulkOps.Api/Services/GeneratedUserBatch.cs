using BulkOps.Api.Entities;

namespace BulkOps.Api.Services;

public class GeneratedUserBatch
{
    public required List<Office> Offices { get; init; }

    public required List<ApplicationUser> ApplicationUsers { get; init; }

    public required List<User> Users { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    public required Dictionary<string, List<string>> UserRoles { get; init; }
}
