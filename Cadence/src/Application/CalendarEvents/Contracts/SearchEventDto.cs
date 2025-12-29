namespace Scheduler.Application.CalendarEvents.Contracts;

public enum SearchEventSource
{
    CalendarEvent = 1,
    Schedule = 2
}

public class SearchEventDto
{
    public Guid Id { get; set; }
    public Guid CalendarId { get; set; }
    public SearchEventSource Source { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsAllDay { get; set; }
    public string? TimeZone { get; set; }
}
