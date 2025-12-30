using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduler.Application.CalendarEvents.Contracts;
using Scheduler.Domain.Services;

namespace Scheduler.Application.CalendarEvents.Queries;

public record GetEventByIdQuery(Guid Id) : IRequest<SearchEventDto?>;

public class GetEventByIdHandler(ISchedulerDbContext db) : IRequestHandler<GetEventByIdQuery, SearchEventDto?>
{
    public async Task<SearchEventDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await db.CalendarEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new SearchEventDto
        {
            Id = entity.Id,
            CalendarId = entity.CalendarId,
            Source = SearchEventSource.CalendarEvent,
            Title = entity.Title,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsAllDay = entity.IsAllDay,
            TimeZone = entity.TimeZone
        };
    }
}
