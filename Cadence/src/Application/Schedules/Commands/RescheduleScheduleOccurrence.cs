using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SharedKernel.Exceptions;
using Scheduler.Domain.Entities.CalendarEvents;

namespace Scheduler.Application.Schedules.Commands;

public class RescheduleScheduleOccurrenceCommand : IRequest<OneOf<Success, NotFoundException, ValidationFailed>>
{
    public Guid ScheduleId { get; set; }
    public DateTime OccurrenceStartDate { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAllDayEvent { get; set; }
    public string? TimeZone { get; set; }
}

public class RescheduleScheduleOccurrenceValidator : AbstractValidator<RescheduleScheduleOccurrenceCommand>
{
    public RescheduleScheduleOccurrenceValidator()
    {
        RuleFor(x => x.ScheduleId).NotEmpty();
        RuleFor(x => x.OccurrenceStartDate).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
    }
}

public class RescheduleScheduleOccurrenceHandler(ISchedulerDbContext db) : IRequestHandler<RescheduleScheduleOccurrenceCommand, OneOf<Success, NotFoundException, ValidationFailed>>
{
    public async Task<OneOf<Success, NotFoundException, ValidationFailed>> Handle(RescheduleScheduleOccurrenceCommand request, CancellationToken cancellationToken)
    {
        var schedule = await db.Schedules.FirstOrDefaultAsync(s => s.Id == request.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return new NotFoundException("Schedule not found");
        }

        var newEventResult = CalendarEvent.Create(
            schedule.CalendarId,
            request.Name,
            request.Description,
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
        return new Success();
    }
}
