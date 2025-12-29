using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduler.Application.Calendars.Contracts;

namespace Scheduler.Application.Calendars.Queries;

public record GetCalendarByIdQuery(Guid Id) : IRequest<CalendarDto?>;

public class GetCalendarByIdHandler(ISchedulerDbContext db) : IRequestHandler<GetCalendarByIdQuery, CalendarDto?>
{
    public async Task<CalendarDto?> Handle(GetCalendarByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await db.Calendars
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity is null)
        { 
            return null; 
        }

        return new CalendarDto(entity.Id, entity.Name, entity.Color);
    }
}
