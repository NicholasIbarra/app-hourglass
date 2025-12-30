using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Application.CalendarEvents.Queries;
using Scheduler.Application.CalendarEvents.Contracts;
using Scheduler.Application.CalendarEvents.Commands;
using OneOf;
using SharedKernel.Exceptions;

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

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SearchEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await mediator.Send(new GetEventByIdQuery(id));
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SearchEventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        var result = await mediator.Send(new CreateEventCommand
        {
            CalendarId = dto.CalendarId,
            Title = dto.Title,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsAllDay = dto.IsAllDay,
            TimeZone = dto.TimeZone
        });

        return result.Match<IActionResult>(
            success => CreatedAtAction(nameof(Get), new { startDate = success.StartDate, endDate = success.EndDate, calendarIds = new[] { success.CalendarId } }, success),
            failed => BadRequest(failed.Message)
        );
    }
}
