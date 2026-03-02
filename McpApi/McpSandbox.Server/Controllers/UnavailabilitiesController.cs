using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Unavailabilities;
using McpSandbox.Server.Data;
using McpSandbox.Server.Domain.Entities.Availabilities;

namespace McpSandbox.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UnavailabilitiesController : ControllerBase
{
    private readonly McpSandboxDbContext _dbContext;

    public UnavailabilitiesController(McpSandboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UnavailabilityDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var unavailability = await _dbContext.Unavailabilities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (unavailability is null)
        {
            return NotFound();
        }

        return Ok(ToDto(unavailability));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UnavailabilityDto>>> Search(
        [FromQuery] Guid? userId,
        [FromQuery] string? search,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Unavailability> query = _dbContext.Unavailabilities.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(u => u.UserId == userId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                (u.Reason != null && u.Reason.Contains(term)) ||
                (u.Notes != null && u.Notes.Contains(term)));
        }

        if (from.HasValue)
        {
            query = query.Where(u => !u.EndDate.HasValue || u.EndDate.Value >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(u => !u.StartDate.HasValue || u.StartDate.Value <= to.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.StartDate)
            .ThenBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<UnavailabilityDto>(
            page,
            pageSize,
            totalCount,
            items.Select(ToDto).ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<UnavailabilityDto>> Create([FromBody] CreateUnavailabilityRequest request, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            return BadRequest("UserId is invalid.");
        }

        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate.Value < request.StartDate.Value)
        {
            return BadRequest("EndDate must be on or after StartDate.");
        }

        var unavailability = new Unavailability
        {
            UserId = request.UserId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsAllDay = request.IsAllDay,
            Reason = request.Reason,
            Notes = request.Notes,
            IsActive = request.IsActive
        };

        _dbContext.Unavailabilities.Add(unavailability);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = unavailability.Id }, ToDto(unavailability));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UnavailabilityDto>> Update(Guid id, [FromBody] UpdateUnavailabilityRequest request, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            return BadRequest("UserId is invalid.");
        }

        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate.Value < request.StartDate.Value)
        {
            return BadRequest("EndDate must be on or after StartDate.");
        }

        var unavailability = await _dbContext.Unavailabilities
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (unavailability is null)
        {
            return NotFound();
        }

        unavailability.UserId = request.UserId;
        unavailability.StartDate = request.StartDate;
        unavailability.EndDate = request.EndDate;
        unavailability.IsAllDay = request.IsAllDay;
        unavailability.Reason = request.Reason;
        unavailability.Notes = request.Notes;
        unavailability.IsActive = request.IsActive;
        unavailability.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(unavailability));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var unavailability = await _dbContext.Unavailabilities
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (unavailability is null)
        {
            return NotFound();
        }

        _dbContext.Unavailabilities.Remove(unavailability);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static UnavailabilityDto ToDto(Unavailability unavailability)
    {
        return new UnavailabilityDto(
            unavailability.Id,
            unavailability.UserId,
            unavailability.StartDate,
            unavailability.EndDate,
            unavailability.IsAllDay,
            unavailability.Reason,
            unavailability.Notes,
            unavailability.IsActive,
            unavailability.CreatedAt,
            unavailability.UpdatedAt);
    }

}
