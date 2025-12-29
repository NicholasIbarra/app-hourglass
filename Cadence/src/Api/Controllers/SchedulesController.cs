using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using Scheduler.Application.Schedules.Commands;
using Scheduler.Application.Schedules.Contracts;
using SharedKernel.Exceptions;

namespace Cadence.Api.Controllers;

[ApiController]
[Route("api/schedules")]
[Produces("application/json")]
public class SchedulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchedulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateScheduleDto dto)
    {
        var result = await _mediator.Send(new CreateScheduleCommand
        {
            CalendarId = dto.CalendarId,
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsAllDayEvent = dto.IsAllDayEvent,
            TimeZone = dto.TimeZone,
            RecurrenceFrequency = dto.RecurrenceFrequency,
            RecurrenceInterval = dto.RecurrenceInterval,
            RecurrenceDayOfWeek = dto.RecurrenceDayOfWeek,
            RecurrenceDayOfMonth = dto.RecurrenceDayOfMonth,
            RecurrenceMonth = dto.RecurrenceMonth,
            RecurrenceOccurrenceCount = dto.RecurrenceOccurrenceCount,
            RecurrenceEndDate = dto.RecurrenceEndDate
        });

        return result.Match<IActionResult>(
            success => CreatedAtAction(nameof(GetById), new { id = success.Id }, success),
            failed => BadRequest(failed.Message)
        );
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Placeholder until query exists
        return NotFound();
    }
}
