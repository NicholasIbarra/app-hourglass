namespace Scheduler.Application.Calendars.Contracts;

public record UpdateCalendarDto
{
    public string Name { get; init; } = "";
    public string? Color { get; init; }
}
