using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SharedKernel.Exceptions;

namespace Scheduler.Application.CalendarEvents.Commands;

public record DeleteEventCommand(Guid Id) : IRequest<OneOf<bool, NotFoundException, ValidationFailed>>;

public class DeleteEventCommandValidator : AbstractValidator<DeleteEventCommand>
{
    public DeleteEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Event ID must be provided.");
    }
}

public class DeleteEventHandler(ISchedulerDbContext db) : IRequestHandler<DeleteEventCommand, OneOf<bool, NotFoundException, ValidationFailed>>
{
    public async Task<OneOf<bool, NotFoundException, ValidationFailed>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.CalendarEvents.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            return new NotFoundException($"Unable to find event with ID {request.Id}");
        }

        db.CalendarEvents.Remove(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            // Likely due to FK constraints (e.g., schedule exceptions referencing this event)
            return new ValidationFailed("Unable to delete event due to existing related data.");
        }
    }
}
