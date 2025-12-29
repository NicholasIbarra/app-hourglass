using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduler.Application.Calendars.Contracts;
using SharedKernel.Queries;
using SharedKernel.Queries.Pagination;

namespace Scheduler.Application.Calendars.Queries;

public record GetAllCalendarsQuery : PaginationQuery, IRequest<IReadOnlyList<CalendarDto>>;

public class GetAllCalendarsQueryValidator : AbstractValidator<GetAllCalendarsQuery>
{
    public GetAllCalendarsQueryValidator(IValidator<PaginationQuery> paginationValidator)
    {
        Include(paginationValidator);

        RuleFor(x => x.SortBy)
            .Must(sortBy => sortBy!.Equals("Name", StringComparison.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage("Sort by must be 'Name'.");

        RuleFor(x => x.SortDirection)
            .IsInEnum()
            .WithMessage("Sort direction must be either 'Asc' or 'Desc'.");
    }
}

public class GetAllCalendarsHandler(ISchedulerDbContext db) : IRequestHandler<GetAllCalendarsQuery, IReadOnlyList<CalendarDto>>
{
    public async Task<IReadOnlyList<CalendarDto>> Handle(GetAllCalendarsQuery request, CancellationToken cancellationToken)
    {
        var queryable = db.Calendars.AsNoTracking()
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        queryable = request.SortBy.ToLower() switch
        {
            "name" => request.SortDirection == SortDirection.Asc
                ? queryable.OrderBy(c => c.Name)
                : queryable.OrderByDescending(c => c.Name),
            _ => queryable.OrderBy(c => c.Name)
        };

        var calendars = await queryable.ToListAsync(cancellationToken);

        return calendars.Select(i => new CalendarDto(i.Id, i.Name, i.Color)).ToList();
    }
}
