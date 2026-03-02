using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using McpSandbox.Server.Data;
using McpSandbox.Server.Domain.Entities.Users;
using ModelContextProtocol.Server;

namespace McpSandbox.Server.Mcp;

[McpServerToolType]
public sealed class UserMcpTools
{
    [McpServerTool(Name = "users_get_by_id"), Description("Gets a user by ID.")]
    public static async Task<UserDto?> GetById(
        McpSandboxDbContext dbContext,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Offices)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user is null ? null : ToDto(user);
    }

    [McpServerTool(Name = "users_search"), Description("Searches users by text with pagination.")]
    public static async Task<PagedResult<UserDto>> Search(
        McpSandboxDbContext dbContext,
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<User> query = dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Name.Contains(term) ||
                (u.Email != null && u.Email.Contains(term)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Include(u => u.Offices)
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>(
            page,
            pageSize,
            totalCount,
            users.Select(ToDto).ToList());
    }

    [McpServerTool(Name = "users_create"), Description("Creates a new user.")]
    public static async Task<UserMutationResult> Create(
        McpSandboxDbContext dbContext,
        CreateUserInput input,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return UserMutationResult.Error("Name is required.");
        }

        var user = new User
        {
            Name = input.Name.Trim(),
            Email = input.Email,
            Type = input.Type,
            IsActive = input.IsActive,
            PhoneNumber = input.PhoneNumber,
            TimeZone = input.TimeZone,
            Locale = input.Locale,
            AvatarUrl = input.AvatarUrl,
            LastLoginAt = input.LastLoginAt
        };

        var officeValidationError = await ApplyOffices(dbContext, user, input.OfficeIds, cancellationToken);
        if (officeValidationError is not null)
        {
            return officeValidationError;
        }

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UserMutationResult.Success(ToDto(user));
    }

    [McpServerTool(Name = "users_update"), Description("Updates an existing user by ID.")]
    public static async Task<UserMutationResult> Update(
        McpSandboxDbContext dbContext,
        Guid id,
        UpdateUserInput input,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return UserMutationResult.Error("Name is required.");
        }

        var user = await dbContext.Users
            .Include(u => u.Offices)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
        {
            return UserMutationResult.Error($"User '{id}' was not found.");
        }

        user.Name = input.Name.Trim();
        user.Email = input.Email;
        user.Type = input.Type;
        user.IsActive = input.IsActive;
        user.PhoneNumber = input.PhoneNumber;
        user.TimeZone = input.TimeZone;
        user.Locale = input.Locale;
        user.AvatarUrl = input.AvatarUrl;
        user.LastLoginAt = input.LastLoginAt;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var officeValidationError = await ApplyOffices(dbContext, user, input.OfficeIds, cancellationToken);
        if (officeValidationError is not null)
        {
            return officeValidationError;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return UserMutationResult.Success(ToDto(user));
    }

    [McpServerTool(Name = "users_delete"), Description("Deletes a user by ID.")]
    public static async Task<UserDeleteResult> Delete(
        McpSandboxDbContext dbContext,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return new UserDeleteResult(false, $"User '{id}' was not found.");
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserDeleteResult(true, null);
    }

    private static async Task<UserMutationResult?> ApplyOffices(
        McpSandboxDbContext dbContext,
        User user,
        IReadOnlyList<Guid>? officeIds,
        CancellationToken cancellationToken)
    {
        var distinctOfficeIds = officeIds?.Distinct().ToList() ?? [];
        var offices = await dbContext.Offices
            .Where(o => distinctOfficeIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        var foundOfficeIds = offices.Select(o => o.Id).ToHashSet();
        var missingOfficeIds = distinctOfficeIds.Where(officeId => !foundOfficeIds.Contains(officeId)).ToList();
        if (missingOfficeIds.Count > 0)
        {
            return UserMutationResult.Error("Some office IDs were not found.", missingOfficeIds);
        }

        user.Offices.Clear();
        foreach (var office in offices)
        {
            user.Offices.Add(office);
        }

        return null;
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Name,
            user.Email,
            user.Type,
            user.IsActive,
            user.PhoneNumber,
            user.TimeZone,
            user.Locale,
            user.AvatarUrl,
            user.LastLoginAt,
            user.CreatedAt,
            user.UpdatedAt,
            user.Offices.Select(o => o.Id).ToList());
    }

    public sealed record CreateUserInput(
        string Name,
        string? Email,
        UserType Type,
        bool IsActive,
        string? PhoneNumber,
        string? TimeZone,
        string? Locale,
        string? AvatarUrl,
        DateTimeOffset? LastLoginAt,
        IReadOnlyList<Guid>? OfficeIds);

    public sealed record UpdateUserInput(
        string Name,
        string? Email,
        UserType Type,
        bool IsActive,
        string? PhoneNumber,
        string? TimeZone,
        string? Locale,
        string? AvatarUrl,
        DateTimeOffset? LastLoginAt,
        IReadOnlyList<Guid>? OfficeIds);

    public sealed record UserDto(
        Guid Id,
        string Name,
        string? Email,
        UserType Type,
        bool IsActive,
        string? PhoneNumber,
        string? TimeZone,
        string? Locale,
        string? AvatarUrl,
        DateTimeOffset? LastLoginAt,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt,
        IReadOnlyList<Guid> OfficeIds);

    public sealed record PagedResult<T>(
        int Page,
        int PageSize,
        int TotalCount,
        IReadOnlyList<T> Items);

    public sealed record UserMutationResult(
        bool Succeeded,
        string? Error,
        UserDto? User,
        IReadOnlyList<Guid>? MissingOfficeIds)
    {
        public static UserMutationResult Success(UserDto user) => new(true, null, user, null);

        public static UserMutationResult Error(string error, IReadOnlyList<Guid>? missingOfficeIds = null)
            => new(false, error, null, missingOfficeIds);
    }

    public sealed record UserDeleteResult(bool Succeeded, string? Error);
}
