namespace Cadence.Api.Models.V1.Calendars;

public class UpdateCalendarRequest
{
    public string Name { get; init; } = "";
    public string? Color { get; init; }
}
