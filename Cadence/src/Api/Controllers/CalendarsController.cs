using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Cadence.Api.Controllers;

[ApiController]
[Route("api/calendars")]
public class CalendarsController : ControllerBase
{
    private readonly IMessageBus _bus;

    public CalendarsController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpGet]
    public async Task<IReadOnlyList<Scheduler.Application.Calendars.CalendarDto>> List()
    {
        //var response = await _bus.InvokeAsync<IReadOnlyList<Scheduler.Application.Calendars.CalendarDto>>(new ListCalendars());
        //return response;
        throw new NotImplementedException();
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
