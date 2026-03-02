using BulkOps.Api.Data;
using BulkOps.Api.Entities;
using BulkOps.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace BulkOps.Api.Repositories;

public class UserBulkRepository(BulkOpsDbContext dbContext, ILogger<UserBulkRepository> logger) : IUserBulkRepository
{
    public async Task<int> BulkImportAsync(GeneratedUserBatch batch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);

        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var sqlConnection = (SqlConnection)dbContext.Database.GetDbConnection();
            var sqlTransaction = (SqlTransaction)transaction.GetDbTransaction();

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
                await dbContext.Offices.AddRangeAsync(newOffices, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                existingOffices.AddRange(newOffices);
            }

            var officeLookup = existingOffices
                .ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

            var roleNames = batch.Roles
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var normalizedRoleNames = roleNames
                .Select(NormalizeRoleName)
                .ToHashSet(StringComparer.Ordinal);

            var existingRoles = await dbContext.Roles
                .Where(x => x.NormalizedName != null && normalizedRoleNames.Contains(x.NormalizedName))
                .ToListAsync(cancellationToken);

            var existingNormalizedRoleNames = existingRoles
                .Select(x => x.NormalizedName!)
                .ToHashSet(StringComparer.Ordinal);

            var missingRoles = roleNames
                .Where(x => !existingNormalizedRoleNames.Contains(NormalizeRoleName(x)))
                .Select(x => new IdentityRole(x)
                {
                    NormalizedName = NormalizeRoleName(x),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                })
                .ToList();

            if (missingRoles.Count > 0)
            {
                await dbContext.Roles.AddRangeAsync(missingRoles, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                existingRoles.AddRange(missingRoles);
            }

            var roleIdByName = existingRoles
                .Where(x => x.Name is not null)
                .ToDictionary(x => x.Name!, x => x.Id, StringComparer.OrdinalIgnoreCase);

            await BulkCopyAsync(sqlConnection, sqlTransaction, "AspNetUsers", CreateApplicationUsersDataTable(batch.ApplicationUsers), 1000, cancellationToken);
            await BulkCopyAsync(sqlConnection, sqlTransaction, "Users", CreateUsersDataTable(batch.Users), 1000, cancellationToken);

            await PopulateUserIdsAsync(batch.Users, cancellationToken);

            var userOffices = batch.Users
                .SelectMany(u => u.UserOffices.Select(uo => new UserOffice
                {
                    UserId = u.Id,
                    OfficeId = officeLookup[uo.Office.Name].Id,
                    AssignedAtUtc = uo.AssignedAtUtc,
                    User = u,
                    Office = officeLookup[uo.Office.Name]
                }))
                .ToList();

            await BulkCopyAsync(sqlConnection, sqlTransaction, "UserOffices", CreateUserOfficesDataTable(userOffices), 2000, cancellationToken);

            var userRoles = CreateUserRolesDataTable(batch.UserRoles, roleIdByName);
            await BulkCopyAsync(sqlConnection, sqlTransaction, "AspNetUserRoles", userRoles, 2000, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation(
                "Bulk imported {UserCount} users with identities, {OfficeCount} offices, {AssignmentCount} user-office assignments, and {UserRoleCount} role assignments.",
                batch.Users.Count,
                existingOffices.Count,
                userOffices.Count,
                userRoles.Rows.Count);

            return batch.Users.Count;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static string NormalizeRoleName(string roleName) => roleName.ToUpperInvariant();

    private async Task PopulateUserIdsAsync(List<User> users, CancellationToken cancellationToken)
    {
        var identityIds = users
            .Select(x => x.IdentityId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .Cast<string>()
            .ToList();

        var userIdLookup = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var chunk in identityIds.Chunk(1000))
        {
            var chunkValues = chunk.ToList();
            var persistedChunk = await dbContext.Users
                .AsNoTracking()
                .Where(x => x.IdentityId != null && chunkValues.Contains(x.IdentityId))
                .Select(x => new { x.IdentityId, x.Id })
                .ToListAsync(cancellationToken);

            foreach (var item in persistedChunk)
            {
                userIdLookup[item.IdentityId!] = item.Id;
            }
        }

        foreach (var user in users)
        {
            if (user.IdentityId is null || !userIdLookup.TryGetValue(user.IdentityId, out var userId))
            {
                throw new InvalidOperationException($"Unable to resolve persisted user id for identity '{user.IdentityId}'.");
            }

            user.Id = userId;
        }
    }

    private static async Task BulkCopyAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string destinationTable,
        DataTable dataTable,
        int batchSize,
        CancellationToken cancellationToken)
    {
        if (dataTable.Rows.Count == 0)
        {
            return;
        }

        using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
        {
            DestinationTableName = destinationTable,
            BatchSize = batchSize
        };

        foreach (DataColumn column in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
    }

    private static DataTable CreateApplicationUsersDataTable(IEnumerable<ApplicationUser> users)
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("UserName", typeof(string));
        table.Columns.Add("NormalizedUserName", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("NormalizedEmail", typeof(string));
        table.Columns.Add("EmailConfirmed", typeof(bool));
        table.Columns.Add("PasswordHash", typeof(string));
        table.Columns.Add("SecurityStamp", typeof(string));
        table.Columns.Add("ConcurrencyStamp", typeof(string));
        table.Columns.Add("PhoneNumberConfirmed", typeof(bool));
        table.Columns.Add("TwoFactorEnabled", typeof(bool));
        table.Columns.Add("AccessFailedCount", typeof(int));
        table.Columns.Add("LockoutEnabled", typeof(bool));

        foreach (var user in users)
        {
            table.Rows.Add(
                user.Id,
                user.UserName,
                user.NormalizedUserName,
                user.Email,
                user.NormalizedEmail,
                user.EmailConfirmed,
                user.PasswordHash,
                user.SecurityStamp,
                user.ConcurrencyStamp,
                user.PhoneNumberConfirmed,
                user.TwoFactorEnabled,
                user.AccessFailedCount,
                user.LockoutEnabled);
        }

        return table;
    }

    private static DataTable CreateUsersDataTable(IEnumerable<User> users)
    {
        var table = new DataTable();
        table.Columns.Add("ExternalId", typeof(string));
        table.Columns.Add("FirstName", typeof(string));
        table.Columns.Add("LastName", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("DateOfBirth", typeof(DateTime));
        table.Columns.Add("IdentityId", typeof(string));

        foreach (var user in users)
        {
            table.Rows.Add(
                user.ExternalId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.DateOfBirth.ToDateTime(TimeOnly.MinValue),
                user.IdentityId);
        }

        return table;
    }

    private static DataTable CreateUserOfficesDataTable(IEnumerable<UserOffice> userOffices)
    {
        var table = new DataTable();
        table.Columns.Add("UserId", typeof(long));
        table.Columns.Add("OfficeId", typeof(int));
        table.Columns.Add("AssignedAtUtc", typeof(DateTime));

        foreach (var userOffice in userOffices)
        {
            table.Rows.Add(userOffice.UserId, userOffice.OfficeId, userOffice.AssignedAtUtc);
        }

        return table;
    }

    private static DataTable CreateUserRolesDataTable(
        Dictionary<string, List<string>> userRoles,
        Dictionary<string, string> roleIdByName)
    {
        var table = new DataTable();
        table.Columns.Add("UserId", typeof(string));
        table.Columns.Add("RoleId", typeof(string));

        var seenAssignments = new HashSet<(string UserId, string RoleId)>();

        foreach (var (userId, roles) in userRoles)
        {
            foreach (var roleName in roles)
            {
                if (!roleIdByName.TryGetValue(roleName, out var roleId))
                {
                    continue;
                }

                var assignment = (userId, roleId);
                if (seenAssignments.Add(assignment))
                {
                    table.Rows.Add(userId, roleId);
                }
            }
        }

        return table;
    }
}
