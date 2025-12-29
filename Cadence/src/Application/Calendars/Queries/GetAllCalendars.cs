using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduler.Application.Calendars.Contracts;
using SharedKernel.Queries;
using SharedKernel.Queries.Pagination;

namespace Scheduler.Application.Calendars.Queries;

public record GetAllCalendarsQuery : PaginationQuery, IRequest<PaginationQueryResponse<CalendarDto>>;

public class GetAllCalendarsQueryValidator : AbstractValidator<GetAllCalendarsQuery>
{
    public string[] SortValues = ["Name"];

    public GetAllCalendarsQueryValidator(IValidator<PaginationQuery> paginationValidator)
    {
        Include(paginationValidator);

        RuleFor(x => x.SortBy)
            .Must(sortBy => SortValues.Contains(sortBy!, StringComparer.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage($"Sort by must be one of the following values: {string.Join(", ", SortValues)}.");
    }
}

public class GetAllCalendarsHandler(ISchedulerDbContext db) : IRequestHandler<GetAllCalendarsQuery, PaginationQueryResponse<CalendarDto>>
{
    public async Task<PaginationQueryResponse<CalendarDto>> Handle(GetAllCalendarsQuery request, CancellationToken cancellationToken)
    {
        var queryable = db.Calendars.AsNoTracking()
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        var total = await queryable.CountAsync(cancellationToken);

        queryable = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == SortDirection.Asc
                ? queryable.OrderBy(c => c.Name)
                : queryable.OrderByDescending(c => c.Name),
            _ => queryable.OrderBy(c => c.Name)
        };

        var calendars = await queryable.ToListAsync(cancellationToken);

        var items = calendars.Select(i => new CalendarDto(i.Id, i.Name, i.Color)).ToList();

        return new PaginationQueryResponse<CalendarDto>(items, total);
    }
}
