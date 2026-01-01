namespace Cadence.Api.Models.V1.Calendars;

public class CreateCalendarRequest
{
    public string Name { get; init; } = "";
    public string? Color { get; init; }
}
