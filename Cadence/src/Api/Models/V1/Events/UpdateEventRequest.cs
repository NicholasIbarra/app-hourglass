namespace Cadence.Api.Models.V1.Events;

public class UpdateEventRequest
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? TimeZone { get; init; }
    public bool IsAllDay { get; init; }
}
