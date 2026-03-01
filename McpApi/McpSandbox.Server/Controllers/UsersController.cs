using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using McpSandbox.Server.Data;
using McpSandbox.Server.Domain.Entities.Users;

namespace McpSandbox.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly McpSandboxDbContext _dbContext;

    public UsersController(McpSandboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(ToDto(user));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> Search(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _dbContext.Users.AsNoTracking();

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
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<UserDto>(
            page,
            pageSize,
            totalCount,
            users.Select(ToDto).ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email,
            Type = request.Type,
            IsActive = request.IsActive,
            PhoneNumber = request.PhoneNumber,
            TimeZone = request.TimeZone,
            Locale = request.Locale,
            AvatarUrl = request.AvatarUrl,
            LastLoginAt = request.LastLoginAt
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToDto(user));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.Name = request.Name.Trim();
        user.Email = request.Email;
        user.Type = request.Type;
        user.IsActive = request.IsActive;
        user.PhoneNumber = request.PhoneNumber;
        user.TimeZone = request.TimeZone;
        user.Locale = request.Locale;
        user.AvatarUrl = request.AvatarUrl;
        user.LastLoginAt = request.LastLoginAt;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(user));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
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
            user.UpdatedAt);
    }

    public sealed record CreateUserRequest(
        string Name,
        string? Email,
        UserType Type,
        bool IsActive,
        string? PhoneNumber,
        string? TimeZone,
        string? Locale,
        string? AvatarUrl,
        DateTimeOffset? LastLoginAt);

    public sealed record UpdateUserRequest(
        string Name,
        string? Email,
        UserType Type,
        bool IsActive,
        string? PhoneNumber,
        string? TimeZone,
        string? Locale,
        string? AvatarUrl,
        DateTimeOffset? LastLoginAt);

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
        DateTimeOffset? UpdatedAt);

    public sealed record PagedResult<T>(
        int Page,
        int PageSize,
        int TotalCount,
        IReadOnlyList<T> Items);
}
