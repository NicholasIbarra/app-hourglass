using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SharedKernel.Exceptions;

namespace Scheduler.Application.Schedules.Commands;

public record DeleteScheduleCommand(Guid Id) : IRequest<OneOf<bool, NotFoundException, ValidationFailed>>;

public class DeleteScheduleCommandValidator : AbstractValidator<DeleteScheduleCommand>
{
    public DeleteScheduleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Schedule ID must be provided.");
    }
}

public class DeleteScheduleHandler(ISchedulerDbContext db) : IRequestHandler<DeleteScheduleCommand, OneOf<bool, NotFoundException, ValidationFailed>>
{
    public async Task<OneOf<bool, NotFoundException, ValidationFailed>> Handle(DeleteScheduleCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Schedules.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            return new NotFoundException($"Unable to find schedule with ID {request.Id}");
        }

        db.Schedules.Remove(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            // Likely due to FK constraints (e.g., existing ScheduleExceptions or related events)
            return new ValidationFailed("Unable to delete schedule due to existing related data.");
        }
    }
}
