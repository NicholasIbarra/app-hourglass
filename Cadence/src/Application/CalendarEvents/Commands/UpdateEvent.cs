using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Scheduler.Application.CalendarEvents.Contracts;
using Scheduler.Domain.Entities.CalendarEvents;
using SharedKernel.Exceptions;

namespace Scheduler.Application.CalendarEvents.Commands;

public class UpdateEventCommand : IRequest<OneOf<SearchEventDto, NotFoundException, ValidationFailed>>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAllDay { get; set; }
    public string? TimeZone { get; set; }
}

public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("Start date must be before end date");
    }
}

public class UpdateEventHandler(ISchedulerDbContext db) : IRequestHandler<UpdateEventCommand, OneOf<SearchEventDto, NotFoundException, ValidationFailed>>
{
    public async Task<OneOf<SearchEventDto, NotFoundException, ValidationFailed>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.CalendarEvents.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            return new NotFoundException("Event not found");
        }

        var reschedule = new EventDate(request.StartDate, request.EndDate);
        entity.Reschedule(reschedule, request.IsAllDay, request.TimeZone);

        var details = entity.UpdateDetails(request.Title, request.Description);
        if (details.IsT1)
        {
            return new ValidationFailed(details.AsT1.Message);
        }

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
