using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduler.Application.Schedules.Contracts;
using SharedKernel.Queries;
using SharedKernel.Queries.Pagination;

namespace Scheduler.Application.Schedules.Queries;

public record GetSchedulesQuery : PaginationQuery, IRequest<PaginationQueryResponse<ScheduleDto>>
{
    public Guid CalendarId { get; init; }
}

public class GetAllSchedulesByCalendarIdQueryValidator : AbstractValidator<GetSchedulesQuery>
{
    public string[] SortValues = ["Name", "StartDate"];

    public GetAllSchedulesByCalendarIdQueryValidator(IValidator<PaginationQuery> paginationValidator)
    {
        Include(paginationValidator);

        RuleFor(x => x.CalendarId)
            .NotEmpty();

        RuleFor(x => x.SortBy)
            .Must(sortBy => SortValues.Contains(sortBy!, StringComparer.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage($"Sort by must be one of the following values: {string.Join(", ", SortValues)}.");
    }
}

public class GetSchedulesHandler(ISchedulerDbContext db) : IRequestHandler<GetSchedulesQuery, PaginationQueryResponse<ScheduleDto>>
{
    public async Task<PaginationQueryResponse<ScheduleDto>> Handle(GetSchedulesQuery request, CancellationToken cancellationToken)
    {
        var queryable = db.Schedules.AsNoTracking()
            .Where(s => s.CalendarId == request.CalendarId);

        var total = await queryable.CountAsync(cancellationToken);

        queryable = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == SortDirection.Asc
                ? queryable.OrderBy(s => s.Title)
                : queryable.OrderByDescending(s => s.Title),
            "startdate" => request.SortDirection == SortDirection.Asc
                ? queryable.OrderBy(s => s.StartDate)
                : queryable.OrderByDescending(s => s.StartDate),
            _ => queryable.OrderBy(s => s.StartDate)
        };

        queryable = queryable
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        var schedules = await queryable.ToListAsync(cancellationToken);

        var items = schedules.Select(entity => new ScheduleDto
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
        }).ToList();

        return new PaginationQueryResponse<ScheduleDto>(items, total);
    }
}
