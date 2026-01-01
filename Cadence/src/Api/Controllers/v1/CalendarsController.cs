using Cadence.Api.Models.V1.Calendars;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Application.Calendars.Commands;
using Scheduler.Application.Calendars.Queries;
using SharedKernel.Queries.Pagination;
using System.ComponentModel.DataAnnotations;

namespace Cadence.Api.Controllers.V1;

[Produces("application/json")]
public class CalendarsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CalendarsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<Scheduler.Application.Calendars.Contracts.CalendarDto>), StatusCodes.Status200OK)]
    public async Task<PagedResponse<Scheduler.Application.Calendars.Contracts.CalendarDto>> List([FromQuery] PageRequest request)
    {
        var result = await _mediator.Send(new GetAllCalendarsQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.Direction
        });

        var pageInfo = new PaginationInfo(request.PageNumber, request.PageSize, result.TotalItems);
        var response = new PagedResponse<Scheduler.Application.Calendars.Contracts.CalendarDto>(result.Items, pageInfo);

        return response;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Scheduler.Application.Calendars.Contracts.CalendarDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCalendarRequest dto)
    {
        var created = await _mediator.Send(new CreateCalendarCommand(dto.Name, dto.Color));

        return created.Match<IActionResult>(
            success => CreatedAtAction(nameof(GetById), new { id = success.Id }, success),
            failed => BadRequest(failed.Message)
        );
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Scheduler.Application.Calendars.Contracts.CalendarDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCalendarByIdQuery(id));
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Scheduler.Application.Calendars.Contracts.CalendarDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([Required] Guid id, [FromBody] UpdateCalendarRequest dto)
    {
        var updated = await _mediator.Send(new UpdateCalendarCommand(id, dto.Name, dto.Color));

        return updated.Match<IActionResult>(
            success => NoContent(),
            failed => BadRequest(failed.Message)
        );
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([Required] Guid id)
    {
        var deleted = await _mediator.Send(new DeleteCalendarCommand(id));

        return deleted.Match<IActionResult>(
            success => NoContent(),
            notFound => NotFound(notFound.Message),
            failed => BadRequest(failed.Message)
        );
    }
}
