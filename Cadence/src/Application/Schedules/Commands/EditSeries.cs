using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Scheduler.Application.Schedules.Contracts;
using Scheduler.Domain.Entities.Schedules;
using SharedKernel.Exceptions;

namespace Scheduler.Application.Schedules.Commands;

public class EditSeriesCommand : IRequest<OneOf<ScheduleDto, NotFoundException, ValidationFailed>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAllDayEvent { get; set; }
    public string TimeZone { get; set; } = "";
    public RecurrenceFrequency RecurrenceFrequency { get; set; }
    public int RecurrenceInterval { get; set; }
    public DayOfTheWeek? RecurrenceDayOfWeek { get; set; }
    public int? RecurrenceDayOfMonth { get; set; }
    public int? RecurrenceMonth { get; set; }
    public int? RecurrenceOccurrenceCount { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
}

public class EditSeriesCommandValidator : AbstractValidator<EditSeriesCommand>
{
    public EditSeriesCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("Start date must be before end date");
        RuleFor(x => x.TimeZone).NotEmpty().WithMessage("Time zone is required");

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

public class EditSeriesHandler(ISchedulerDbContext db) : IRequestHandler<EditSeriesCommand, OneOf<ScheduleDto, NotFoundException, ValidationFailed>>
{
    public async Task<OneOf<ScheduleDto, NotFoundException, ValidationFailed>> Handle(EditSeriesCommand request, CancellationToken cancellationToken)
    {
        var schedule = await db.Schedules.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        
        if (schedule is null)
        {
            return new NotFoundException("Schedule not found");
        }

        var updateSeries = schedule.UpdateSeries(new ScheduleDate(request.StartDate, request.EndDate), request.IsAllDayEvent, request.TimeZone);
        if (updateSeries.IsT1)
        {
            return new ValidationFailed(updateSeries.AsT1.Message);
        }

        var details = schedule.UpdateDetails(request.Name, request.Description);
        if (details.IsT1)
        {
            return new ValidationFailed(details.AsT1.Message);
        }

        var setRecurrence = schedule.SetRecurrencePattern(
            request.RecurrenceFrequency,
            request.RecurrenceFrequency == RecurrenceFrequency.None ? 1 : request.RecurrenceInterval,
            request.RecurrenceDayOfWeek,
            request.RecurrenceDayOfMonth,
            request.RecurrenceMonth,
            request.RecurrenceOccurrenceCount,
            request.RecurrenceEndDate);

        if (setRecurrence.IsT1)
        {
            return new ValidationFailed(setRecurrence.AsT1.Message);
        }

        await db.SaveChangesAsync(cancellationToken);

        var dto = new ScheduleDto
        {
            Id = schedule.Id,
            CalendarId = schedule.CalendarId,
            Name = schedule.Title,
            Description = schedule.Description,
            StartDate = schedule.StartDate,
            EndDate = schedule.EndDate,
            IsAllDayEvent = schedule.IsAllDay,
            TimeZone = schedule.TimeZone ?? string.Empty,
            RecurrenceFrequency = schedule.RecurrencePattern.Frequency,
            RecurrenceInterval = schedule.RecurrencePattern.Interval,
            RecurrenceDayOfWeek = schedule.RecurrencePattern.DayOfWeek,
            RecurrenceDayOfMonth = schedule.RecurrencePattern.DayOfMonth,
            RecurrenceMonth = schedule.RecurrencePattern.Month,
            RecurrenceOccurrenceCount = schedule.RecurrencePattern.OccurrenceCount,
            RecurrenceEndDate = schedule.RecurrenceEndDate
        };

        return dto;
    }
}
