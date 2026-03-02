using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using McpSandbox.Server.Data;
using McpSandbox.Server.Domain.Entities.Availabilities;
using McpSandbox.Server.Domain.Entities.Availabilities.ValueObjects;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpSandbox.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AvailabilitiesController : ControllerBase
{
    private readonly McpSandboxDbContext _dbContext;

    public AvailabilitiesController(McpSandboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AvailabilityDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var availability = await _dbContext.Availabilities
            .AsNoTracking()
            .Include(a => a.Offices)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (availability is null)
        {
            return NotFound();
        }

        return Ok(ToDto(availability));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AvailabilityDto>>> Search(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? officeId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Availability> query = _dbContext.Availabilities.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        if (officeId.HasValue)
        {
            query = query.Where(a => a.Offices.Any(o => o.Id == officeId.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(a => a.Offices)
            .OrderBy(a => a.EffectiveFrom)
            .ThenBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<AvailabilityDto>(
            page,
            pageSize,
            totalCount,
            items.Select(ToDto).ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<AvailabilityDto>> Create([FromBody] CreateAvailabilityRequest request, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            return BadRequest("UserId is invalid.");
        }

        if (request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom)
        {
            return BadRequest("EffectiveTo must be on or after EffectiveFrom.");
        }

        if (!TryBuildWeeklySchedule(request, out var weeklySchedule, out var validationError))
        {
            return BadRequest(validationError);
        }

        var officeIds = request.OfficeIds?.Distinct().ToList() ?? [];
        var offices = await _dbContext.Offices
            .Where(o => officeIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        var foundOfficeIds = offices.Select(o => o.Id).ToHashSet();
        var missingOfficeIds = officeIds.Where(officeId => !foundOfficeIds.Contains(officeId)).ToList();
        if (missingOfficeIds.Count > 0)
        {
            return BadRequest(new { Message = "Some office IDs were not found.", OfficeIds = missingOfficeIds });
        }

        var availability = new Availability
        {
            UserId = request.UserId,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            IsActive = request.IsActive,
            Sunday = weeklySchedule.Sunday,
            Monday = weeklySchedule.Monday,
            Tuesday = weeklySchedule.Tuesday,
            Wednesday = weeklySchedule.Wednesday,
            Thursday = weeklySchedule.Thursday,
            Friday = weeklySchedule.Friday,
            Saturday = weeklySchedule.Saturday,
            Offices = offices
        };

        _dbContext.Availabilities.Add(availability);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = availability.Id }, ToDto(availability));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AvailabilityDto>> Update(Guid id, [FromBody] UpdateAvailabilityRequest request, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            return BadRequest("UserId is invalid.");
        }

        if (request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom)
        {
            return BadRequest("EffectiveTo must be on or after EffectiveFrom.");
        }

        if (!TryBuildWeeklySchedule(request, out var weeklySchedule, out var validationError))
        {
            return BadRequest(validationError);
        }

        var availability = await _dbContext.Availabilities
            .Include(a => a.Offices)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (availability is null)
        {
            return NotFound();
        }

        var officeIds = request.OfficeIds?.Distinct().ToList() ?? [];
        var offices = await _dbContext.Offices
            .Where(o => officeIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        var foundOfficeIds = offices.Select(o => o.Id).ToHashSet();
        var missingOfficeIds = officeIds.Where(officeId => !foundOfficeIds.Contains(officeId)).ToList();
        if (missingOfficeIds.Count > 0)
        {
            return BadRequest(new { Message = "Some office IDs were not found.", OfficeIds = missingOfficeIds });
        }

        availability.UserId = request.UserId;
        availability.EffectiveFrom = request.EffectiveFrom;
        availability.EffectiveTo = request.EffectiveTo;
        availability.IsActive = request.IsActive;
        availability.Sunday = weeklySchedule.Sunday;
        availability.Monday = weeklySchedule.Monday;
        availability.Tuesday = weeklySchedule.Tuesday;
        availability.Wednesday = weeklySchedule.Wednesday;
        availability.Thursday = weeklySchedule.Thursday;
        availability.Friday = weeklySchedule.Friday;
        availability.Saturday = weeklySchedule.Saturday;
        availability.UpdatedAt = DateTimeOffset.UtcNow;

        availability.Offices.Clear();
        foreach (var office in offices)
        {
            availability.Offices.Add(office);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(availability));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var availability = await _dbContext.Availabilities
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (availability is null)
        {
            return NotFound();
        }

        _dbContext.Availabilities.Remove(availability);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static AvailabilityDto ToDto(Availability availability)
    {
        return new AvailabilityDto(
            availability.Id,
            availability.UserId,
            availability.EffectiveFrom,
            availability.EffectiveTo,
            availability.IsActive,
            ToDayRangeDto(availability.Sunday),
            ToDayRangeDto(availability.Monday),
            ToDayRangeDto(availability.Tuesday),
            ToDayRangeDto(availability.Wednesday),
            ToDayRangeDto(availability.Thursday),
            ToDayRangeDto(availability.Friday),
            ToDayRangeDto(availability.Saturday),
            availability.Offices.Select(o => o.Id).ToList(),
            availability.CreatedAt,
            availability.UpdatedAt);
    }

    private static DayTimeRangeDto? ToDayRangeDto(AvailabilityDayTimeRange? range)
    {
        if (!range.HasValue)
        {
            return null;
        }

        return new DayTimeRangeDto(range.Value.StartTime, range.Value.EndTime);
    }

    private static bool TryBuildWeeklySchedule(AvailabilityRequestBase request, out WeeklySchedule schedule, out string? error)
    {
        schedule = default;

        if (!TryCreateRange(request.Sunday, DayOfWeek.Sunday, out var sunday, out error) ||
            !TryCreateRange(request.Monday, DayOfWeek.Monday, out var monday, out error) ||
            !TryCreateRange(request.Tuesday, DayOfWeek.Tuesday, out var tuesday, out error) ||
            !TryCreateRange(request.Wednesday, DayOfWeek.Wednesday, out var wednesday, out error) ||
            !TryCreateRange(request.Thursday, DayOfWeek.Thursday, out var thursday, out error) ||
            !TryCreateRange(request.Friday, DayOfWeek.Friday, out var friday, out error) ||
            !TryCreateRange(request.Saturday, DayOfWeek.Saturday, out var saturday, out error))
        {
            return false;
        }

        schedule = new WeeklySchedule(sunday, monday, tuesday, wednesday, thursday, friday, saturday);
        return true;
    }

    private static bool TryCreateRange(
        DayTimeRangeDto? value,
        DayOfWeek expectedDayOfWeek,
        out AvailabilityDayTimeRange? range,
        out string? error)
    {
        range = null;
        error = null;

        if (value is null)
        {
            return true;
        }

        try
        {
            range = new AvailabilityDayTimeRange(expectedDayOfWeek, value.StartTime, value.EndTime);
            return true;
        }
        catch (ArgumentException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public abstract record AvailabilityRequestBase(
        Guid UserId,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        bool IsActive,
        DayTimeRangeDto? Sunday,
        DayTimeRangeDto? Monday,
        DayTimeRangeDto? Tuesday,
        DayTimeRangeDto? Wednesday,
        DayTimeRangeDto? Thursday,
        DayTimeRangeDto? Friday,
        DayTimeRangeDto? Saturday,
        IReadOnlyList<Guid>? OfficeIds);

    public sealed record CreateAvailabilityRequest(
        Guid UserId,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        bool IsActive,
        DayTimeRangeDto? Sunday,
        DayTimeRangeDto? Monday,
        DayTimeRangeDto? Tuesday,
        DayTimeRangeDto? Wednesday,
        DayTimeRangeDto? Thursday,
        DayTimeRangeDto? Friday,
        DayTimeRangeDto? Saturday,
        IReadOnlyList<Guid>? OfficeIds) : AvailabilityRequestBase(
            UserId,
            EffectiveFrom,
            EffectiveTo,
            IsActive,
            Sunday,
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            OfficeIds);

    public sealed record UpdateAvailabilityRequest(
        Guid UserId,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        bool IsActive,
        DayTimeRangeDto? Sunday,
        DayTimeRangeDto? Monday,
        DayTimeRangeDto? Tuesday,
        DayTimeRangeDto? Wednesday,
        DayTimeRangeDto? Thursday,
        DayTimeRangeDto? Friday,
        DayTimeRangeDto? Saturday,
        IReadOnlyList<Guid>? OfficeIds) : AvailabilityRequestBase(
            UserId,
            EffectiveFrom,
            EffectiveTo,
            IsActive,
            Sunday,
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            OfficeIds);

    public sealed record AvailabilityDto(
        Guid Id,
        Guid UserId,
        DateOnly EffectiveFrom,
        DateOnly? EffectiveTo,
        bool IsActive,
        DayTimeRangeDto? Sunday,
        DayTimeRangeDto? Monday,
        DayTimeRangeDto? Tuesday,
        DayTimeRangeDto? Wednesday,
        DayTimeRangeDto? Thursday,
        DayTimeRangeDto? Friday,
        DayTimeRangeDto? Saturday,
        IReadOnlyList<Guid> OfficeIds,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    public sealed record DayTimeRangeDto(
        [property: JsonConverter(typeof(FlexibleTimeOnlyJsonConverter))] TimeOnly StartTime,
        [property: JsonConverter(typeof(FlexibleTimeOnlyJsonConverter))] TimeOnly EndTime);

    public sealed record PagedResult<T>(
        int Page,
        int PageSize,
        int TotalCount,
        IReadOnlyList<T> Items);

    private readonly record struct WeeklySchedule(
        AvailabilityDayTimeRange? Sunday,
        AvailabilityDayTimeRange? Monday,
        AvailabilityDayTimeRange? Tuesday,
        AvailabilityDayTimeRange? Wednesday,
        AvailabilityDayTimeRange? Thursday,
        AvailabilityDayTimeRange? Friday,
        AvailabilityDayTimeRange? Saturday);

    private sealed class FlexibleTimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new JsonException("Time value is required.");
            }

            if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeOnly))
            {
                return timeOnly;
            }

            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTimeOffset))
            {
                return TimeOnly.FromTimeSpan(dateTimeOffset.TimeOfDay);
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
            {
                return TimeOnly.FromDateTime(dateTime);
            }

            throw new JsonException($"Invalid time value '{value}'.");
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        }
    }
}
