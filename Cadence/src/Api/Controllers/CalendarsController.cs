using Microsoft.AspNetCore.Mvc;
using MediatR;
using Scheduler.Application.Calendars.Contracts;
using Scheduler.Application.Calendars.Queries;
using SharedKernel.Queries.Pagination;

namespace Cadence.Api.Controllers;

[ApiController]
[Route("api/calendars")]
public class CalendarsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CalendarsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IReadOnlyList<CalendarDto>> List([FromQuery] PageRequest request)
    {
        var response = await _mediator.Send(new GetAllCalendarsQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.Direction
        });
        return response;
    }

    //[HttpGet("{id}")]
    //public async Task<ActionResult<Scheduler.Application.Calendars.CalendarDto>> Get(Guid id)
    //{
    //    var result = await _bus.InvokeAsync<Scheduler.Application.Calendars.CalendarDto?>(new GetCalendar(id));
    //    if (result == null) return NotFound();
    //    return Ok(result);
    //}

    //[HttpPost]
    //public async Task<Scheduler.Application.Calendars.CalendarDto> Create([FromBody] CreateCalendar command)
    //    => await _bus.InvokeAsync<Scheduler.Application.Calendars.CalendarDto>(command);

    //[HttpPut("{id}")]
    //public async Task<Scheduler.Application.Calendars.CalendarDto> Update(Guid id, [FromBody] Scheduler.Application.Calendars.Commands.UpdateCalendar body)
    //    => await _bus.InvokeAsync<Scheduler.Application.Calendars.CalendarDto>(body with { Id = id });

    //[HttpDelete("{id}")]
    //public async Task<IActionResult> Delete(Guid id)
    //{
    //    await _bus.InvokeAsync(new DeleteCalendar(id));
    //    return NoContent();
    //}
}
