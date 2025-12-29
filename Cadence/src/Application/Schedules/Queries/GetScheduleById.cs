using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduler.Application.Schedules.Contracts;

namespace Scheduler.Application.Schedules.Queries;

public record GetScheduleByIdQuery(Guid Id) : IRequest<ScheduleDto?>;

public class GetScheduleByIdHandler(ISchedulerDbContext db) : IRequestHandler<GetScheduleByIdQuery, ScheduleDto?>
{
    public async Task<ScheduleDto?> Handle(GetScheduleByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await db.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new ScheduleDto
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
    }
}