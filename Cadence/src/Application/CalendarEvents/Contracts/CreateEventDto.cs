namespace Scheduler.Application.CalendarEvents.Contracts;

public record CreateEventDto
{
    public Guid CalendarId { get; init; }
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsAllDay { get; init; }
    public string? TimeZone { get; init; }
}
