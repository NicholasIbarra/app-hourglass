namespace Scheduler.Application.Calendars.Contracts;

public record CreateCalendarDto
{
    public string Name { get; init; } = "";
    public string? Color { get; init; }
}
