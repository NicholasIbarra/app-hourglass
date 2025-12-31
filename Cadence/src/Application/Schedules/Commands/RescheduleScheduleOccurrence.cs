using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Scheduler.Application.CalendarEvents.Contracts;
using Scheduler.Domain.Entities.CalendarEvents;
using Scheduler.Domain.Entities.Schedules;
using Scheduler.Domain.Managers;
using SharedKernel.Exceptions;

namespace Scheduler.Application.Schedules.Commands;

public class RescheduleScheduleOccurrenceCommand : IRequest<OneOf<SearchEventDto, NotFoundException, ValidationFailed>>
{
    public Guid ScheduleId { get; set; }
    public DateTime OccurrenceStartDate { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAllDayEvent { get; set; }
    public string? TimeZone { get; set; }
}

public class RescheduleScheduleOccurrenceValidator : AbstractValidator<RescheduleScheduleOccurrenceCommand>
{
    private readonly ISchedulerDbContext _db;
    private readonly IScheduledEventResolver _resolver;

    public RescheduleScheduleOccurrenceValidator(ISchedulerDbContext db, IScheduledEventResolver resolver)
    {
        _db = db;
        _resolver = resolver;

        RuleFor(x => x.ScheduleId).NotEmpty();
        RuleFor(x => x.OccurrenceStartDate).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();

        RuleFor(x => x)
            .MustAsync(NotBeforeSeriesStart)
            .WithMessage("Occurrence date is before the series start date.");

        RuleFor(x => x)
            .MustAsync(NotAfterSeriesEnd)
            .WithMessage("Occurrence date is after the series end date.");

        RuleFor(x => x)
            .MustAsync(OccurrenceBelongsToSeries)
            .WithMessage("Occurrence date is not in the series.");
    }

    private async Task<bool> NotBeforeSeriesStart(RescheduleScheduleOccurrenceCommand request, CancellationToken ct)
    {
        var schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.Id == request.ScheduleId, ct);
        if (schedule is null) return true; // handler will return NotFound
        return request.OccurrenceStartDate >= schedule.StartDate;
    }

    private async Task<bool> NotAfterSeriesEnd(RescheduleScheduleOccurrenceCommand request, CancellationToken ct)
    {
        var schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.Id == request.ScheduleId, ct);
        if (schedule is null) return true; // handler will return NotFound
        if (!schedule.RecurrenceEndDate.HasValue) return true;
        return request.OccurrenceStartDate <= schedule.RecurrenceEndDate.Value;
    }

    private async Task<bool> OccurrenceBelongsToSeries(RescheduleScheduleOccurrenceCommand request, CancellationToken ct)
    {
        var schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.Id == request.ScheduleId, ct);
        if (schedule is null) return true; // handler will return NotFound

        var resolved = _resolver.Resolve(schedule, request.OccurrenceStartDate, request.OccurrenceStartDate);
        var hasOccurrence = resolved.Any(e => e.OccursAt == request.OccurrenceStartDate && e.Type == ScheduledEventInstanceType.Pseudo);
        return hasOccurrence;
    }
}

public class RescheduleScheduleOccurrenceHandler(ISchedulerDbContext db, IScheduledEventResolver resolver) : IRequestHandler<RescheduleScheduleOccurrenceCommand, OneOf<SearchEventDto, NotFoundException, ValidationFailed>>
{
    public async Task<OneOf<SearchEventDto, NotFoundException, ValidationFailed>> Handle(RescheduleScheduleOccurrenceCommand request, CancellationToken cancellationToken)
    {
        var schedule = await db.Schedules.FirstOrDefaultAsync(s => s.Id == request.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return new NotFoundException("Schedule not found");
        }

        var title = string.IsNullOrWhiteSpace(request.Name) ? schedule.Title : request.Name;
        var description = request.Description ?? schedule.Description;

        var newEventResult = CalendarEvent.Create(
            schedule.CalendarId,
            title,
            description,
            new EventDate(request.StartDate, request.EndDate),
            request.IsAllDayEvent,
            request.TimeZone,
            schedule.Id);

        if (newEventResult.IsT1)
        {
            return new ValidationFailed(newEventResult.AsT1.Message);
        }

        var newEvent = newEventResult.AsT0;
        await db.CalendarEvents.AddAsync(newEvent, cancellationToken);

        var seriesResult = schedule.Reschedule(request.OccurrenceStartDate, newEvent.Id);
        if (seriesResult.IsT1)
        {
            return new ValidationFailed(seriesResult.AsT1.Message);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new SearchEventDto
        {
            Id = newEvent.Id,
            CalendarId = newEvent.CalendarId,
            Source = SearchEventSource.CalendarEvent,
            Title = newEvent.Title,
            Description = newEvent.Description,
            StartDate = newEvent.StartDate,
            EndDate = newEvent.EndDate,
            IsAllDay = newEvent.IsAllDay,
            TimeZone = newEvent.TimeZone
        };
    }
}
