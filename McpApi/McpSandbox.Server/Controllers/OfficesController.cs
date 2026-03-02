using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using McpSandbox.Server.Data;
using McpSandbox.Server.Domain.Entities.Offices;

namespace McpSandbox.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OfficesController : ControllerBase
{
    private readonly McpSandboxDbContext _dbContext;

    public OfficesController(McpSandboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OfficeDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var office = await _dbContext.Offices
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (office is null)
        {
            return NotFound();
        }

        return Ok(ToDto(office));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OfficeDto>>> Search(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Office> query = _dbContext.Offices.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(o =>
                o.Name.Contains(term) ||
                (o.Code != null && o.Code.Contains(term)) ||
                (o.PhoneNumber != null && o.PhoneNumber.Contains(term)) ||
                (o.City != null && o.City.Contains(term)) ||
                (o.StateOrProvince != null && o.StateOrProvince.Contains(term)) ||
                (o.Country != null && o.Country.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var offices = await query
            .OrderBy(o => o.Name)
            .ThenBy(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<OfficeDto>(
            page,
            pageSize,
            totalCount,
            offices.Select(ToDto).ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<OfficeDto>> Create([FromBody] CreateOfficeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var office = new Office
        {
            Name = request.Name.Trim(),
            Code = request.Code,
            IsActive = request.IsActive,
            PhoneNumber = request.PhoneNumber,
            TimeZone = request.TimeZone,
            Notes = request.Notes,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            StateOrProvince = request.StateOrProvince,
            PostalCode = request.PostalCode,
            Country = request.Country
        };

        _dbContext.Offices.Add(office);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = office.Id }, ToDto(office));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OfficeDto>> Update(Guid id, [FromBody] UpdateOfficeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var office = await _dbContext.Offices.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (office is null)
        {
            return NotFound();
        }

        office.Name = request.Name.Trim();
        office.Code = request.Code;
        office.IsActive = request.IsActive;
        office.PhoneNumber = request.PhoneNumber;
        office.TimeZone = request.TimeZone;
        office.Notes = request.Notes;
        office.AddressLine1 = request.AddressLine1;
        office.AddressLine2 = request.AddressLine2;
        office.City = request.City;
        office.StateOrProvince = request.StateOrProvince;
        office.PostalCode = request.PostalCode;
        office.Country = request.Country;
        office.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(office));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var office = await _dbContext.Offices.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (office is null)
        {
            return NotFound();
        }

        _dbContext.Offices.Remove(office);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static OfficeDto ToDto(Office office)
    {
        return new OfficeDto(
            office.Id,
            office.Name,
            office.Code,
            office.IsActive,
            office.PhoneNumber,
            office.TimeZone,
            office.Notes,
            office.AddressLine1,
            office.AddressLine2,
            office.City,
            office.StateOrProvince,
            office.PostalCode,
            office.Country,
            office.CreatedAt,
            office.UpdatedAt);
    }

    public sealed record CreateOfficeRequest(
        string Name,
        string? Code,
        bool IsActive,
        string? PhoneNumber,
        string? TimeZone,
        string? Notes,
        string? AddressLine1,
        string? AddressLine2,
        string? City,
        string? StateOrProvince,
        string? PostalCode,
        string? Country);

    public sealed record UpdateOfficeRequest(
        string Name,
        string? Code,
        bool IsActive,
        string? PhoneNumber,
        string? TimeZone,
        string? Notes,
        string? AddressLine1,
        string? AddressLine2,
        string? City,
        string? StateOrProvince,
        string? PostalCode,
        string? Country);

    public sealed record OfficeDto(
        Guid Id,
        string Name,
        string? Code,
        bool IsActive,
        string? PhoneNumber,
        string? TimeZone,
        string? Notes,
        string? AddressLine1,
        string? AddressLine2,
        string? City,
        string? StateOrProvince,
        string? PostalCode,
        string? Country,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    public sealed record PagedResult<T>(
        int Page,
        int PageSize,
        int TotalCount,
        IReadOnlyList<T> Items);
}
