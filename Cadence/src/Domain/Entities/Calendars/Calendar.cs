using OneOf;
using Scheduler.Domain.Services;
using Shared.Entities;

namespace Scheduler.Domain.Entities.Calendars;

public class Calendar : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = "";

    public string? Color { get; private set; }

    public static OneOf<Calendar, ArgumentException, FormatException> Create(string name, string? color)
    {
        color = color ?? ColorService.GenerateRandomHexColor();

        if (!ColorService.IsValidHexColor(color))
        {
            return new FormatException("Invalid calendar color provided");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return new ArgumentException("Name cannot be empty or whitespace.", nameof(name));
        }

        var calendar = new Calendar
        {
            Id = Guid.NewGuid(),
            Name = name,
            Color = color,
        };

        return calendar;
    }
}
