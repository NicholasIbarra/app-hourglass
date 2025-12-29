using FluentValidation;
using MediatR;
using Scheduler.Domain.Entities.Schedules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scheduler.Application.Schedules.Commands;

public record ScheduleDto
{
    public Guid Id { get; init; }
    public Guid CalendarId { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsAllDayEvent { get; init; }
    public string TimeZone { get; init; } = "";
    public RecurrenceFrequency RecurrenceFrequency { get; init; }
    public int RecurrenceInterval { get; init; }
    public DayOfTheWeek? RecurrenceDayOfWeek { get; init; }
    public int? RecurrenceDayOfMonth { get; init; }
    public int? RecurrenceMonth { get; init; }
    public int? RecurrenceOccurrenceCount { get; init; }
    public DateTime? RecurrenceEndDate { get; init; }
}

public class CreateScheduleCommand : IRequest<ScheduleDto>
{
    public Guid CalendarId { get; set; }
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

public class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleCommandValidator()
    {
        RuleFor(x => x.CalendarId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("Start date must be before end date");
        RuleFor(x => x.TimeZone).NotEmpty().WithMessage("Time zone is required");
    }
}

public class CreateScheduleHandler (ISchedulerDbContext context): IRequestHandler<CreateScheduleCommand, ScheduleDto>
{
    public Task<ScheduleDto> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        // Implementation would go here
        throw new NotImplementedException();
    }
}