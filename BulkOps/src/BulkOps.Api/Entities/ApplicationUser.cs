using Microsoft.AspNetCore.Identity;

namespace BulkOps.Api.Entities;

public class ApplicationUser : IdentityUser
{
    public User? User { get; set; }
}
