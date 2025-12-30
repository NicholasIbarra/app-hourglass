using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SharedKernel.Exceptions;

namespace Scheduler.Application.Calendars.Commands;

public record DeleteCalendarCommand(Guid Id) : IRequest<OneOf<bool, NotFoundException, ValidationFailed>>;

public class DeleteCalendarCommandValidator : AbstractValidator<DeleteCalendarCommand>
{
    public DeleteCalendarCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Calendar ID must be provided.");
    }
}

public class DeleteCalendarHandler(ISchedulerDbContext db) : IRequestHandler<DeleteCalendarCommand, OneOf<bool, NotFoundException, ValidationFailed>>
{
    public async Task<OneOf<bool, NotFoundException, ValidationFailed>> Handle(DeleteCalendarCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Calendars.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            return new NotFoundException($"Unable to find calendar with ID {request.Id}");
        }

        db.Calendars.Remove(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            // Likely due to FK constraints (e.g., existing CalendarEvents)
            return new ValidationFailed("Unable to delete calendar due to existing related data.");
        }
    }
}
