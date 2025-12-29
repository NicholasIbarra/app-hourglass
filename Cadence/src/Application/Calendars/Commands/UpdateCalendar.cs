using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Scheduler.Application.Calendars.Contracts;
using FluentValidation;
using SharedKernel.Exceptions;

namespace Scheduler.Application.Calendars.Commands;

public record UpdateCalendarCommand(Guid Id, string Name, string? Color) : IRequest<OneOf<CalendarDto, ValidationFailed>>;

public class UpdateCalendarCommandValidator : AbstractValidator<UpdateCalendarCommand>
{
    public UpdateCalendarCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Color)
            .Must(color => color is null || Scheduler.Domain.Services.ColorService.IsValidHexColor(color))
            .WithMessage("Invalid calendar color provided");
    }
}

public class UpdateCalendarHandler(ISchedulerDbContext db, IValidator<UpdateCalendarCommand> validator) : IRequestHandler<UpdateCalendarCommand, OneOf<CalendarDto, ValidationFailed>>
{
    public async Task<OneOf<CalendarDto, ValidationFailed>> Handle(UpdateCalendarCommand request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return new ValidationFailed(message);
        }

        var calendar = await db.Calendars.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (calendar is null) throw new NotFoundException();

        var result = calendar.Update(request.Name, request.Color);
        if (result.IsT1) return new ValidationFailed(result.AsT1.Message);
        if (result.IsT2) return new ValidationFailed(result.AsT2.Message);

        await db.SaveChangesAsync(cancellationToken);

        return new CalendarDto(calendar.Id, calendar.Name, calendar.Color);
    }
}
