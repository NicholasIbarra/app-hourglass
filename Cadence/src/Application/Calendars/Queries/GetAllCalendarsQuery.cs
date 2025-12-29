using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Scheduler.Application.Calendars;

public record GetAllCalendarsQuery : IRequest<IReadOnlyList<CalendarDto>>
{
    public int PageNumber { get; init; } = 1;
}

public class GetAllCalendarsQueryValidator : AbstractValidator<GetAllCalendarsQuery>
{
    public GetAllCalendarsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");
    }
}

public class GetAllCalendarsHandler(ISchedulerDbContext db) : IRequestHandler<GetAllCalendarsQuery, IReadOnlyList<CalendarDto>>
{
    public async Task<IReadOnlyList<CalendarDto>> Handle(GetAllCalendarsQuery request, CancellationToken cancellationToken)
    {
        var calendars = await db.Calendars.AsNoTracking()
            .Skip((request.PageNumber - 1) * 10)
            .Take(10)
            .ToListAsync(cancellationToken);

        return calendars.Select(CalendarMappings.ToDto).ToList();
    }
}
