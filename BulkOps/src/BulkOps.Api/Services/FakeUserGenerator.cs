using Bogus;
using BulkOps.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace BulkOps.Api.Services;

public class FakeUserGenerator : IFakeUserGenerator
{
    private static readonly string[] OfficeNames =
    [
        "HQ",
        "North Hub",
        "South Hub",
        "East Hub",
        "West Hub",
        "R&D",
        "Operations",
        "Finance",
        "Support",
        "Remote"
    ];

    public GeneratedUserBatch Generate(int userCount)
    {
        if (userCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userCount), "User count must be greater than zero.");
        }

        var faker = new Faker();
        var passwordHasher = new PasswordHasher<ApplicationUser>();

        var offices = OfficeNames
            .Select(name => new Office
            {
                Name = name,
                City = faker.Address.City()
            })
            .ToList();

        var userFaker = new Faker<User>()
            .RuleFor(x => x.ExternalId, _ => Guid.NewGuid().ToString("N"))
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.Email, (f, x) => f.Internet.Email(x.FirstName, x.LastName))
            .RuleFor(x => x.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Past(45, DateTime.UtcNow.AddYears(-18))));

        var users = userFaker.Generate(userCount);

        var applicationUsers = new List<ApplicationUser>(userCount);

        foreach (var user in users)
        {
            var appUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = user.Email,
                NormalizedUserName = user.Email.ToUpperInvariant(),
                Email = user.Email,
                NormalizedEmail = user.Email.ToUpperInvariant(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            appUser.PasswordHash = passwordHasher.HashPassword(appUser, "Password123!");

            user.IdentityId = appUser.Id;
            applicationUsers.Add(appUser);

            var office = faker.PickRandom(offices);
            user.UserOffices.Add(new UserOffice
            {
                User = user,
                Office = office,
                AssignedAtUtc = DateTime.UtcNow
            });
        }

        return new GeneratedUserBatch
        {
            Offices = offices,
            ApplicationUsers = applicationUsers,
            Users = users
        };
    }
}

