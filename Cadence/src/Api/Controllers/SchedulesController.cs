using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Application.Schedules.Commands;
using Scheduler.Application.Schedules.Contracts;
using Scheduler.Application.Schedules.Extensions;
using Scheduler.Application.Schedules.Queries;
using Scheduler.Domain.Entities.Schedules;
using SharedKernel.Queries.Pagination;

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
        var weeklyFlags = WeekdayFlagsExtensions.ToDayOfTheWeekFlags(
            dto.IsSunday,
            dto.IsMonday,
            dto.IsTuesday,
            dto.IsWednesday,
            dto.IsThursday,
            dto.IsFriday,
            dto.IsSaturday,
            dto.RecurrenceFrequency);

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
            RecurrenceDayOfWeek = weeklyFlags,
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

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditSeries(Guid id, [FromBody] EditSeriesDto dto)
    {
        var weeklyFlags = WeekdayFlagsExtensions.ToDayOfTheWeekFlags(
            dto.IsSunday,
            dto.IsMonday,
            dto.IsTuesday,
            dto.IsWednesday,
            dto.IsThursday,
            dto.IsFriday,
            dto.IsSaturday,
            dto.RecurrenceFrequency);

        var result = await _mediator.Send(new EditSeriesCommand
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsAllDayEvent = dto.IsAllDayEvent,
            TimeZone = dto.TimeZone,
            RecurrenceFrequency = dto.RecurrenceFrequency,
            RecurrenceInterval = dto.RecurrenceInterval,
            RecurrenceDayOfWeek = weeklyFlags,
            RecurrenceDayOfMonth = dto.RecurrenceDayOfMonth,
            RecurrenceMonth = dto.RecurrenceMonth,
            RecurrenceOccurrenceCount = dto.RecurrenceOccurrenceCount,
            RecurrenceEndDate = dto.RecurrenceEndDate
        });

        return result.Match<IActionResult>(
            _ => Ok(),
            notFound => NotFound(),
            failed => BadRequest(failed.Message)
        );
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetScheduleByIdQuery(id));
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCalendarId(Guid calendarId, [FromQuery] PageRequest request)
    {
        var result = await _mediator.Send(new GetSchedulesQuery
        {
            CalendarId = calendarId,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.Direction
        });

        var pageInfo = new PaginationInfo(request.PageNumber, request.PageSize, result.TotalItems);
        var response = new PagedResponse<ScheduleDto>(result.Items, pageInfo);

        return Ok(response);
    }
}
