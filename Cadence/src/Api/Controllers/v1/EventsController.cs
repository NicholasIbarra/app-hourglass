using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Application.CalendarEvents.Commands;
using Scheduler.Application.CalendarEvents.Queries;
using Cadence.Api.Models.V1.Events;
using Scheduler.Application.CalendarEvents.Contracts;

namespace Cadence.Api.Controllers.V1;

[Produces("application/json")]
public class EventsController(IMediator mediator) : ApiControllerBase
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
    public async Task<IActionResult> Create([FromBody] CreateEventRequest dto)
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

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateEventRequest dto)
    {
        var result = await mediator.Send(new UpdateEventCommand
        {
            Id = id,
            Title = dto.Title,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsAllDay = dto.IsAllDay,
            TimeZone = dto.TimeZone
        });

        return result.Match<IActionResult>(
            success => NoContent(),
            notFound => NotFound(notFound.Message),
            failed => BadRequest(failed.Message)
        );
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await mediator.Send(new DeleteEventCommand(id));
        return result.Match<IActionResult>(
            success => NoContent(),
            notFound => NotFound(notFound.Message),
            failed => BadRequest(failed.Message)
        );
    }
}
