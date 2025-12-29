using MediatR;
using OneOf;
using Scheduler.Application.Calendars.Contracts;
using Scheduler.Domain.Entities.Calendars;
using FluentValidation;
using SharedKernel.Exceptions;

namespace Scheduler.Application.Calendars.Commands;

public record CreateCalendarCommand(string Name, string? Color) : IRequest<OneOf<CalendarDto, ValidationFailed>>;

public class CreateCalendarCommandValidator : AbstractValidator<CreateCalendarCommand>
{
    public CreateCalendarCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Color)
            .Must(color => color is null || Scheduler.Domain.Services.ColorService.IsValidHexColor(color))
            .WithMessage("Invalid calendar color provided");
    }
}

public class CreateCalendarHandler(ISchedulerDbContext db, IValidator<CreateCalendarCommand> validator) : IRequestHandler<CreateCalendarCommand, OneOf<CalendarDto, ValidationFailed>>
{
    public async Task<OneOf<CalendarDto, ValidationFailed>> Handle(CreateCalendarCommand request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return new ValidationFailed(message);
        }

        var created = Calendar.Create(request.Name, request.Color);
        if (created.IsT1)
            return new ValidationFailed(created.AsT1.Message);
        if (created.IsT2)
            return new ValidationFailed(created.AsT2.Message);

        var entity = created.AsT0;
        db.Calendars.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new CalendarDto(entity.Id, entity.Name, entity.Color);
    }
}
