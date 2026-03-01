using BulkOps.Api.Data;
using BulkOps.Api.Entities;
using BulkOps.Api.Services;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace BulkOps.Api.Repositories;

public class UserBulkRepository(BulkOpsDbContext dbContext, ILogger<UserBulkRepository> logger) : IUserBulkRepository
{
    public async Task<int> BulkImportAsync(GeneratedUserBatch batch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);

        var officeNames = batch.Offices.Select(x => x.Name).Distinct().ToList();

        var existingOffices = await dbContext.Offices
            .Where(x => officeNames.Contains(x.Name))
            .ToListAsync(cancellationToken);

        var existingOfficeNames = existingOffices
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newOffices = batch.Offices
            .Where(x => !existingOfficeNames.Contains(x.Name))
            .ToList();

        if (newOffices.Count > 0)
        {
            await dbContext.BulkInsertAsync(newOffices, new BulkConfig
            {
                SetOutputIdentity = true,
                PreserveInsertOrder = true
            }, cancellationToken: cancellationToken);

            existingOffices.AddRange(newOffices);
        }

        var officeLookup = existingOffices
            .ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

        await dbContext.BulkInsertAsync(batch.Users, new BulkConfig
        {
            BatchSize = 1000,
            PreserveInsertOrder = true,
            SetOutputIdentity = true
        }, cancellationToken: cancellationToken);

        var assignments = batch.UserOffices
            .Select(x =>
            {
                var office = officeLookup[x.Office.Name];

                return new UserOffice
                {
                    UserId = x.User.Id,
                    OfficeId = office.Id,
                    AssignedAtUtc = x.AssignedAtUtc,
                    User = x.User,
                    Office = office
                };
            })
            .ToList();

        await dbContext.BulkInsertAsync(assignments, new BulkConfig
        {
            BatchSize = 2000,
            PreserveInsertOrder = true,
            SetOutputIdentity = false
        }, cancellationToken: cancellationToken);

        logger.LogInformation(
            "Bulk imported {UserCount} users, {OfficeCount} offices, and {AssignmentCount} user-office assignments.",
            batch.Users.Count,
            existingOffices.Count,
            assignments.Count);

        return batch.Users.Count;
    }
}
