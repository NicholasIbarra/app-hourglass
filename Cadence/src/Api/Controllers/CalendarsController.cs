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
    public async Task<PagedResponse<CalendarDto>> List([FromQuery] PageRequest request)
    {
        var result = await _mediator.Send(new GetAllCalendarsQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.Direction
        });

        var pageInfo = new PaginationInfo(request.PageNumber, request.PageSize, result.TotalItems);
        var response = new PagedResponse<CalendarDto>(result.Items, pageInfo);

        return response;
    }
}
