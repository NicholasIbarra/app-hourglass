using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SharedKernel.Exceptions;

namespace Scheduler.Application.Schedules.Commands;

public class SkipScheduleOccurrenceCommand : IRequest<OneOf<Success, NotFoundException, ValidationFailed>>
{
    public Guid ScheduleId { get; set; }
    public DateTime OccurrenceStartDate { get; set; }
}

public class SkipScheduleOccurrenceValidator : AbstractValidator<SkipScheduleOccurrenceCommand>
{
    public SkipScheduleOccurrenceValidator()
    {
        RuleFor(x => x.ScheduleId).NotEmpty();
        RuleFor(x => x.OccurrenceStartDate).NotEmpty();
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
