using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SharedKernel.Exceptions;
using Scheduler.Domain.Managers;
using Scheduler.Domain.Entities.Schedules;

namespace Scheduler.Application.Schedules.Commands;

public class SkipScheduleOccurrenceCommand : IRequest<OneOf<Success, NotFoundException, ValidationFailed>>
{
    public Guid ScheduleId { get; set; }
    public DateTime OccurrenceStartDate { get; set; }
}

public class SkipScheduleOccurrenceValidator : AbstractValidator<SkipScheduleOccurrenceCommand>
{
    private readonly ISchedulerDbContext _db;
    private readonly IScheduledEventResolver _resolver;

    public SkipScheduleOccurrenceValidator(ISchedulerDbContext db, IScheduledEventResolver resolver)
    {
        _db = db;
        _resolver = resolver;

        RuleFor(x => x.ScheduleId).NotEmpty();
        RuleFor(x => x.OccurrenceStartDate).NotEmpty();

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

    private async Task<bool> NotBeforeSeriesStart(SkipScheduleOccurrenceCommand request, CancellationToken ct)
    {
        var schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.Id == request.ScheduleId, ct);
        if (schedule is null) return true; // handler will return NotFound
        return request.OccurrenceStartDate >= schedule.StartDate;
    }

    private async Task<bool> NotAfterSeriesEnd(SkipScheduleOccurrenceCommand request, CancellationToken ct)
    {
        var schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.Id == request.ScheduleId, ct);
        if (schedule is null) return true; // handler will return NotFound
        if (!schedule.RecurrenceEndDate.HasValue) return true;
        return request.OccurrenceStartDate <= schedule.RecurrenceEndDate.Value;
    }

    private async Task<bool> OccurrenceBelongsToSeries(SkipScheduleOccurrenceCommand request, CancellationToken ct)
    {
        var schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.Id == request.ScheduleId, ct);
        if (schedule is null) return true; // handler will return NotFound

        var resolved = _resolver.Resolve(schedule, request.OccurrenceStartDate, request.OccurrenceStartDate);
        var hasOccurrence = resolved.Any(e => e.OccursAt == request.OccurrenceStartDate && e.Type == ScheduledEventInstanceType.Pseudo);
        return hasOccurrence;
    }
}

public class SkipScheduleOccurrenceHandler(ISchedulerDbContext db) : IRequestHandler<SkipScheduleOccurrenceCommand, OneOf<Success, NotFoundException, ValidationFailed>>
{
    public async Task<OneOf<Success, NotFoundException, ValidationFailed>> Handle(SkipScheduleOccurrenceCommand request, CancellationToken cancellationToken)
    {
        var schedule = await db.Schedules.FirstOrDefaultAsync(s => s.Id == request.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return new NotFoundException("Schedule not found");
        }

        var result = schedule.Skip(request.OccurrenceStartDate);
        if (result.IsT1)
        {
            return new ValidationFailed(result.AsT1.Message);
        }

        await db.SaveChangesAsync(cancellationToken);
        return new Success();
    }
}
