using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduler.Application.CalendarEvents.Contracts;
using Scheduler.Domain.Managers;
using Scheduler.Domain.Entities.Schedules;

namespace Scheduler.Application.CalendarEvents.Queries;

public record SearchEventsQuery(DateTime StartDate, DateTime EndDate, Guid[]? CalendarIds) : IRequest<List<SearchEventDto>>;

public class SearchEventsQueryValidator : AbstractValidator<SearchEventsQuery>
{
    public SearchEventsQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("StartDate must be earlier than EndDate.");

        RuleFor(x => x.CalendarIds)
            .Must(ids => ids == null || ids.Length > 0)
            .WithMessage("CalendarIds must be null or contain at least one ID.");
    }
}


public class SearchEventsHandler(ISchedulerDbContext db, IScheduledEventResolver resolver) : IRequestHandler<SearchEventsQuery, List<SearchEventDto>>
{
    public async Task<List<SearchEventDto>> Handle(SearchEventsQuery request, CancellationToken cancellationToken)
    {
        var start = request.StartDate;
        var end = request.EndDate;
        var ids = request.CalendarIds;

        var eventsQuery = db.CalendarEvents.AsNoTracking()
            .Where(e => e.StartDate <= end && e.EndDate >= start)
            .Where(e => ids!.Contains(e.CalendarId));

        var concreteEvents = await eventsQuery
            .Select(e => new SearchEventDto
            {
                Id = e.Id,
                CalendarId = e.CalendarId,
                Source = SearchEventSource.CalendarEvent,
                Title = e.Title,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsAllDay = e.IsAllDay,
                TimeZone = e.TimeZone
            })
            .ToListAsync(cancellationToken);

        var schedulesQuery = db.Schedules.AsNoTracking();

        var schedules = await schedulesQuery
            .Where(e => ids!.Contains(e.CalendarId))
            .ToListAsync(cancellationToken);

        var pseudoEvents = new List<SearchEventDto>();

        foreach (var s in schedules)
        {
            var instances = resolver.Resolve(s, start, end);
            if (instances.Count == 0)
                continue;

            var duration = s.EndDate - s.StartDate;

            foreach (var inst in instances)
            {
                if (inst.Type != ScheduledEventInstanceType.Pseudo)
                    continue; // skip materialized/rescheduled; concrete events already included above

                var occurrenceStart = inst.OccursAt;
                var occurrenceEnd = occurrenceStart + duration;

                // Safety overlap check
                if (occurrenceStart <= end && occurrenceEnd >= start)
                {
                    pseudoEvents.Add(new SearchEventDto
                    {
                        Id = s.Id,
                        CalendarId = s.CalendarId,
                        Source = SearchEventSource.Schedule,
                        Title = s.Title,
                        Description = s.Description,
                        StartDate = occurrenceStart,
                        EndDate = occurrenceEnd,
                        IsAllDay = s.IsAllDay,
                        TimeZone = s.TimeZone
                    });
                }
            }
        }

        return concreteEvents.Concat(pseudoEvents)
            .OrderBy(i => i.StartDate)
            .ToList();
    }
}
