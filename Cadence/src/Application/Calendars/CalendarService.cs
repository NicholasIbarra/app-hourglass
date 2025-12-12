using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Scheduler.Domain.Entities.Calendars;

namespace Scheduler.Application.Calendars;

public interface ICalendarService
{
}

public record CreateCalendarDto
{
    public string Name { get; init; } = "";
    public string? Color { get; init; }
}

public class CalendarService : ICalendarService
{
    private readonly ILogger<CalendarService> _logger;



    public CalendarService(ILogger<CalendarService> logger)
    {
        _logger = logger;
    }
}

public record CalendarDto(Guid Id, string Name, string? Color);

public static class CalendarMappings
{
    public static CalendarDto ToDto(this Calendar c) => new(c.Id, c.Name, c.Color);
}
