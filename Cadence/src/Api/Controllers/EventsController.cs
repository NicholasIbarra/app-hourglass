using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Application.CalendarEvents.Queries;
using Scheduler.Application.CalendarEvents.Contracts;

namespace Cadence.Api.Controllers;

[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<SearchEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid[]? calendarIds)
    {
        if (endDate < startDate)
        {
            return BadRequest("End date must be greater than or equal to start date.");
        }

        var results = await mediator.Send(new SearchEventsQuery(startDate, endDate, calendarIds));
        return Ok(results);
    }
}
