using FluentValidation;
using MediatR;
using OneOf;
using Scheduler.Application.Schedules.Contracts;
using Scheduler.Domain.Entities.Schedules;
using SharedKernel.Exceptions;

namespace Scheduler.Application.Schedules.Commands;

public class CreateScheduleCommand : IRequest<OneOf<ScheduleDto, ValidationFailed>>
{
    public Guid CalendarId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAllDayEvent { get; set; }
    public string? TimeZone { get; set; }
    public RecurrenceFrequency RecurrenceFrequency { get; set; }
    public int RecurrenceInterval { get; set; }
    public DayOfTheWeek? RecurrenceDayOfWeek { get; set; }
    public int? RecurrenceDayOfMonth { get; set; }
    public int? RecurrenceMonth { get; set; }
    public int? RecurrenceOccurrenceCount { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
}

public class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleCommandValidator()
    {
        RuleFor(x => x.CalendarId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("Start date must be before end date");
        //RuleFor(x => x.TimeZone).NotEmpty().WithMessage("Time zone is required");

        RuleFor(x => x.RecurrenceFrequency)
            .NotNull().WithMessage("Recurrence frequency is required")
            .IsInEnum().WithMessage("Valid recurrence frequency is required");

        RuleFor(x => x.RecurrenceInterval)
            .GreaterThan(0).WithMessage("Recurrence interval must be greater than 0")
            .When(x => x.RecurrenceFrequency != RecurrenceFrequency.None);

        RuleFor(x => x.RecurrenceDayOfWeek)
            .Must((command, dayOfWeek) => command.RecurrenceFrequency != RecurrenceFrequency.Weekly || dayOfWeek.HasValue)
            .WithMessage("Day of the week must be provided for weekly recurrence")
            .When(x => x.RecurrenceFrequency != RecurrenceFrequency.None);

        RuleFor(x => x.RecurrenceDayOfMonth)
            .Must((command, dayOfMonth) => command.RecurrenceFrequency != RecurrenceFrequency.Monthly || dayOfMonth.HasValue)
            .WithMessage("Day of the month must be provided for monthly recurrence")
            .When(x => x.RecurrenceFrequency != RecurrenceFrequency.None);

        RuleFor(x => x.RecurrenceMonth)
            .Must((command, month) => command.RecurrenceFrequency != RecurrenceFrequency.Yearly || month.HasValue)
            .WithMessage("Month must be provided for yearly recurrence")
            .When(x => x.RecurrenceFrequency != RecurrenceFrequency.None);

        RuleFor(x => x.RecurrenceOccurrenceCount)
            .GreaterThan(0).When(x => x.RecurrenceOccurrenceCount.HasValue)
            .WithMessage("Recurrence occurrence count must be greater than 0")
            .When(x => x.RecurrenceFrequency != RecurrenceFrequency.None);

        RuleFor(x => x.RecurrenceEndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.RecurrenceEndDate.HasValue)
            .WithMessage("Recurrence end date cannot be before the event start date")
            .When(x => x.RecurrenceFrequency != RecurrenceFrequency.None);
    }
}

public class CreateScheduleHandler(ISchedulerDbContext db, IValidator<CreateScheduleCommand> validator) : IRequestHandler<CreateScheduleCommand, OneOf<ScheduleDto, ValidationFailed>>
{
    public async Task<OneOf<ScheduleDto, ValidationFailed>> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
            return new ValidationFailed(message);
        }

        var recurrence = RecurrencePattern.Create
        (
            request.RecurrenceFrequency,
            request.RecurrenceInterval,
            request.RecurrenceDayOfWeek,
            request.RecurrenceDayOfMonth,
            request.RecurrenceMonth,
            request.RecurrenceOccurrenceCount
        );

        if (recurrence.IsT1)
        {
            return new ValidationFailed(recurrence.AsT1.Message);
        }

        var startEnd = new ScheduleDate(request.StartDate, request.EndDate);

        var created = Schedule.Create(
            request.CalendarId,
            request.Name,
            request.Description,
            startEnd,
            request.IsAllDayEvent,
            request.TimeZone,
            recurrence.AsT0,
            request.RecurrenceEndDate);

        if (created.IsT1)
        {
            return new ValidationFailed(created.AsT1.Message);
        }

        var entity = created.AsT0;
        db.Schedules.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var dto = new ScheduleDto
        {
            Id = entity.Id,
            CalendarId = entity.CalendarId,
            Name = entity.Title,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsAllDayEvent = entity.IsAllDay,
            TimeZone = entity.TimeZone ?? string.Empty,
            RecurrenceFrequency = entity.RecurrencePattern.Frequency,
            RecurrenceInterval = entity.RecurrencePattern.Interval,
            RecurrenceDayOfWeek = entity.RecurrencePattern.DayOfWeek,
            RecurrenceDayOfMonth = entity.RecurrencePattern.DayOfMonth,
            RecurrenceMonth = entity.RecurrencePattern.Month,
            RecurrenceOccurrenceCount = entity.RecurrencePattern.OccurrenceCount,
            RecurrenceEndDate = entity.RecurrenceEndDate
        };

        return dto;
    }
}