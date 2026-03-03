using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Availabilities;
using McpSandbox.Api.Contracts.Schedule;
using McpSandbox.Server.Data;
using McpSandbox.Server.Domain.Entities.Availabilities;
using McpSandbox.Server.Domain.Entities.Availabilities.ValueObjects;
using McpSandbox.Server.Domain.Entities.ShiftRequests;

namespace McpSandbox.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly McpSandboxDbContext _dbContext;

    public ScheduleController(McpSandboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/schedule")]
    public IActionResult GetSchedule()
    {
        var schedule = new
        {
            Monday = "Math",
            Tuesday = "Science",
            Wednesday = "History",
            Thursday = "Art",
            Friday = "Physical Education"
        };
        return Ok(schedule);
    }

    /// <summary>
    /// Returns users who are available for a given date range and office,
    /// excluding those with overlapping unavailabilities or existing shift request assignments.
    /// </summary>
    [HttpGet("availability")]
    public async Task<ActionResult<IReadOnlyList<ScheduleAvailabilityDto>>> GetAvailability(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid officeId,
        CancellationToken cancellationToken)
    {
        if (endDate < startDate)
        {
            return BadRequest("endDate must be on or after startDate.");
        }

        var availableUserIds = await GetAvailableUserIdsAsync(startDate, endDate, officeId, cancellationToken);

        if (availableUserIds.Count == 0)
        {
            return Ok(Array.Empty<ScheduleAvailabilityDto>());
        }

        var results = await _dbContext.Users
            .AsNoTracking()
            .Where(u => availableUserIds.Contains(u.Id))
            .Select(u => new ScheduleAvailabilityDto(u.Id, u.Name))
            .ToListAsync(cancellationToken);

        return Ok(results);
    }

    [HttpPost("shift-request")]
    public async Task<ActionResult<ShiftRequestDto>> CreateShiftRequest(
        [FromBody] CreateShiftRequestRequest request,
        CancellationToken cancellationToken)
    {
        if (request.EndAt <= request.StartAt)
        {
            return BadRequest("EndAt must be after StartAt.");
        }

        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return BadRequest("User not found.");
        }

        var officeExists = await _dbContext.Offices
            .AnyAsync(o => o.Id == request.OfficeId, cancellationToken);
        if (!officeExists)
        {
            return BadRequest("Office not found.");
        }

        var startDate = DateOnly.FromDateTime(request.StartAt.UtcDateTime);
        var endDate = DateOnly.FromDateTime(request.EndAt.UtcDateTime);
        var availableUserIds = await GetAvailableUserIdsAsync(startDate, endDate, request.OfficeId, cancellationToken, [request.UserId]);
        if (!availableUserIds.Contains(request.UserId))
        {
            return UnprocessableEntity("The user is not available at the requested office for the given date range.");
        }

        if (!await IsShiftWithinAvailabilityHoursAsync(request.UserId, request.OfficeId, request.StartAt, request.EndAt, cancellationToken))
        {
            return UnprocessableEntity("The shift times fall outside the user's available hours.");
        }

        var shiftRequest = new ShiftRequest
        {
            UserId = request.UserId,
            OfficeId = request.OfficeId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Reason = request.Reason,
            Notes = request.Notes
        };

        _dbContext.ShiftRequests.Add(shiftRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetShiftRequestById), new { id = shiftRequest.Id }, ToDto(shiftRequest));
    }

    [HttpGet("shift-request/{id:guid}")]
    public async Task<ActionResult<ShiftRequestDto>> GetShiftRequestById(Guid id, CancellationToken cancellationToken)
    {
        var shiftRequest = await _dbContext.ShiftRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(sr => sr.Id == id, cancellationToken);

        if (shiftRequest is null)
        {
            return NotFound();
        }

        return Ok(ToDto(shiftRequest));
    }

    [HttpGet("shift-request")]
    public async Task<ActionResult<PagedResult<ShiftRequestDto>>> SearchShiftRequests(
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? officeId = null,
        [FromQuery] ShiftRequestStatus? status = null,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<ShiftRequest> query = _dbContext.ShiftRequests.AsNoTracking();

        if (userId.HasValue)
            query = query.Where(sr => sr.UserId == userId.Value);

        if (officeId.HasValue)
            query = query.Where(sr => sr.OfficeId == officeId.Value);

        if (status.HasValue)
            query = query.Where(sr => sr.Status == status.Value);

        if (startDate.HasValue)
        {
            var startOffset = new DateTimeOffset(startDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(sr => sr.EndAt >= startOffset);
        }

        if (endDate.HasValue)
        {
            var endOffset = new DateTimeOffset(endDate.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            query = query.Where(sr => sr.StartAt <= endOffset);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(sr => sr.StartAt)
            .ThenBy(sr => sr.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<ShiftRequestDto>(
            page,
            pageSize,
            totalCount,
            items.Select(ToDto).ToList()));
    }

    [HttpPut("shift-request/{id:guid}/status")]
    public async Task<ActionResult<ShiftRequestDto>> UpdateShiftRequestStatus(
        Guid id,
        [FromBody] UpdateShiftRequestStatusRequest request,
        CancellationToken cancellationToken)
    {
        var shiftRequest = await _dbContext.ShiftRequests
            .FirstOrDefaultAsync(sr => sr.Id == id, cancellationToken);

        if (shiftRequest is null)
        {
            return NotFound();
        }

        shiftRequest.Status = request.Status;
        if (request.Notes is not null)
            shiftRequest.Notes = request.Notes;
        shiftRequest.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(shiftRequest));
    }

    [HttpPost("shift-request/{id:guid}/cancel")]
    public async Task<ActionResult<ShiftRequestDto>> CancelShiftRequest(
        Guid id,
        [FromBody] CancelShiftRequestRequest? request,
        CancellationToken cancellationToken)
    {
        var shiftRequest = await _dbContext.ShiftRequests
            .FirstOrDefaultAsync(sr => sr.Id == id, cancellationToken);

        if (shiftRequest is null)
        {
            return NotFound();
        }

        shiftRequest.Status = ShiftRequestStatus.Cancelled;
        if (request?.Notes is not null)
            shiftRequest.Notes = request.Notes;
        shiftRequest.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(shiftRequest));
    }

    [HttpDelete("shift-request/{id:guid}")]
    public async Task<IActionResult> DeleteShiftRequest(Guid id, CancellationToken cancellationToken)
    {
        var shiftRequest = await _dbContext.ShiftRequests
            .FirstOrDefaultAsync(sr => sr.Id == id, cancellationToken);

        if (shiftRequest is null)
        {
            return NotFound();
        }

        _dbContext.ShiftRequests.Remove(shiftRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<HashSet<Guid>> GetAvailableUserIdsAsync(
        DateOnly startDate,
        DateOnly endDate,
        Guid officeId,
        CancellationToken cancellationToken,
        IReadOnlyCollection<Guid>? candidateUserIds = null)
    {
        // 1. Find active availabilities that overlap the requested date range and apply to the office.
        //    An availability with no offices defined is treated as covering all of the user's offices.
        var query = _dbContext.Availabilities
            .AsNoTracking()
            .Where(a => a.IsActive
                && a.EffectiveFrom <= endDate
                && (a.EffectiveTo == null || a.EffectiveTo >= startDate)
                && (!a.Offices.Any()
                    ? a.User.Offices.Any(o => o.Id == officeId)
                    : a.Offices.Any(o => o.Id == officeId)));

        if (candidateUserIds is { Count: > 0 })
            query = query.Where(a => candidateUserIds.Contains(a.UserId));

        var userIds = await query
            .Select(a => a.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (userIds.Count == 0)
            return [];

        // 2. Find users who have an active unavailability overlapping the date range.
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

        var unavailableUserIds = await _dbContext.Unavailabilities
            .AsNoTracking()
            .Where(u => u.IsActive
                && userIds.Contains(u.UserId)
                && (u.StartDate == null || u.StartDate <= endDateTime)
                && (u.EndDate == null || u.EndDate >= startDateTime))
            .Select(u => u.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 3. Find users already assigned to an approved/pending shift request at this office in the date range.
        var startOffset = new DateTimeOffset(startDateTime, TimeSpan.Zero);
        var endOffset = new DateTimeOffset(endDateTime, TimeSpan.Zero);

        var assignedUserIds = await _dbContext.ShiftRequests
            .AsNoTracking()
            .Where(sr => sr.OfficeId == officeId
                && userIds.Contains(sr.UserId)
                && sr.Status != ShiftRequestStatus.Rejected
                && sr.Status != ShiftRequestStatus.Cancelled
                && sr.StartAt < endOffset
                && sr.EndAt > startOffset)
            .Select(sr => sr.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 4. Return user IDs that are not excluded.
        var excluded = new HashSet<Guid>(unavailableUserIds.Concat(assignedUserIds));
        return userIds.Where(id => !excluded.Contains(id)).ToHashSet();
    }

    private async Task<bool> IsShiftWithinAvailabilityHoursAsync(
        Guid userId,
        Guid officeId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        CancellationToken cancellationToken)
    {
        var startDate = DateOnly.FromDateTime(startAt.UtcDateTime);
        var endDate = DateOnly.FromDateTime(endAt.UtcDateTime);

        // endAt landing exactly on midnight logically belongs to the previous day
        if (endAt.UtcDateTime.TimeOfDay == TimeSpan.Zero)
            endDate = endDate.AddDays(-1);

        var availabilities = await _dbContext.Availabilities
            .AsNoTracking()
            .Where(a => a.UserId == userId
                && a.IsActive
                && a.EffectiveFrom <= endDate
                && (a.EffectiveTo == null || a.EffectiveTo >= startDate)
                && (!a.Offices.Any()
                    ? a.User.Offices.Any(o => o.Id == officeId)
                    : a.Offices.Any(o => o.Id == officeId)))
            .ToListAsync(cancellationToken);

        if (availabilities.Count == 0)
            return false;

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            // On the first day use the actual start time; on intermediate days the shift starts at midnight.
            var shiftStartOnDay = date == startDate
                ? TimeOnly.FromDateTime(startAt.UtcDateTime)
                : TimeOnly.MinValue;

            // On the last day use the actual end time; on intermediate days the shift runs to end-of-day.
            var shiftEndOnDay = date == endDate
                ? TimeOnly.FromDateTime(endAt.UtcDateTime)
                : new TimeOnly(23, 59, 59);

            var coveredOnDay = availabilities.Any(a =>
            {
                if (a.EffectiveFrom > date || (a.EffectiveTo is not null && a.EffectiveTo < date))
                    return false;

                var dayRange = GetDayTimeRange(a, date.DayOfWeek);
                return dayRange is not null
                    && shiftStartOnDay >= dayRange.Value.StartTime
                    && shiftEndOnDay <= dayRange.Value.EndTime;
            });

            if (!coveredOnDay)
                return false;
        }

        return true;
    }

    private static AvailabilityDayTimeRange? GetDayTimeRange(Availability availability, DayOfWeek dayOfWeek) =>
        dayOfWeek switch
        {
            DayOfWeek.Sunday => availability.Sunday,
            DayOfWeek.Monday => availability.Monday,
            DayOfWeek.Tuesday => availability.Tuesday,
            DayOfWeek.Wednesday => availability.Wednesday,
            DayOfWeek.Thursday => availability.Thursday,
            DayOfWeek.Friday => availability.Friday,
            DayOfWeek.Saturday => availability.Saturday,
            _ => null
        };

    private static ShiftRequestDto ToDto(ShiftRequest sr) => new(
        sr.Id,
        sr.UserId,
        sr.OfficeId,
        sr.StartAt,
        sr.EndAt,
        sr.Status,
        sr.RequestedAt,
        sr.Reason,
        sr.Notes);
}
