using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpSandbox.Api.Contracts.Availabilities;

public sealed record DayTimeRangeDto(
    [property: JsonConverter(typeof(FlexibleTimeOnlyJsonConverter))] TimeOnly StartTime,
    [property: JsonConverter(typeof(FlexibleTimeOnlyJsonConverter))] TimeOnly EndTime);

public sealed class FlexibleTimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("Time value is required.");
        }

        if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeOnly))
        {
            return timeOnly;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTimeOffset))
        {
            return TimeOnly.FromTimeSpan(dateTimeOffset.TimeOfDay);
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
        {
            return TimeOnly.FromDateTime(dateTime);
        }

        throw new JsonException($"Invalid time value '{value}'.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
    }
}
