using FluentValidation;
using MediatR;
using OneOf;
using Scheduler.Application.CalendarEvents.Contracts;
using Scheduler.Domain.Entities.CalendarEvents;
using SharedKernel.Exceptions;

namespace Scheduler.Application.CalendarEvents.Commands;

public class CreateEventCommand : IRequest<OneOf<SearchEventDto, ValidationFailed>>
{
    public Guid CalendarId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAllDay { get; set; }
    public string? TimeZone { get; set; }
}

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.CalendarId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("Start date must be before end date");
    }
}

public class CreateEventHandler(ISchedulerDbContext db) : IRequestHandler<CreateEventCommand, OneOf<SearchEventDto, ValidationFailed>>
{
    public async Task<OneOf<SearchEventDto, ValidationFailed>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var startEnd = new EventDate(request.StartDate, request.EndDate);
        var created = CalendarEvent.Create(
            request.CalendarId,
            request.Title,
            request.Description,
            startEnd,
            request.IsAllDay,
            request.TimeZone,
            null);

        if (created.IsT1)
        {
            return new ValidationFailed(created.AsT1.Message);
        }

        var entity = created.AsT0;
        db.CalendarEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var dto = new SearchEventDto
        {
            Id = entity.Id,
            CalendarId = entity.CalendarId,
            Source = SearchEventSource.CalendarEvent,
            Title = entity.Title,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsAllDay = entity.IsAllDay,
            TimeZone = entity.TimeZone
        };

        return dto;
    }
}
