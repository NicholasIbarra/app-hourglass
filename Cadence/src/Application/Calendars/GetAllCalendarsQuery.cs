using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduler.Domain.Entities.Calendars;

namespace Scheduler.Application.Calendars;

public record GetAllCalendarsQuery : IRequest<IReadOnlyList<CalendarDto>>;

public class GetAllCalendarsHandler(ISchdulerDbContext db) : IRequestHandler<GetAllCalendarsQuery, IReadOnlyList<CalendarDto>>
{
    public async Task<IReadOnlyList<CalendarDto>> Handle(GetAllCalendarsQuery request, CancellationToken cancellationToken)
    {
        var calendars = await db.Calendars.AsNoTracking().ToListAsync(cancellationToken);
        return calendars.Select(CalendarMappings.ToDto).ToList();
    }
}
