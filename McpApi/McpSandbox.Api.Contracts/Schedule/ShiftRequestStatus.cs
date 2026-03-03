using System.Text.Json.Serialization;

namespace McpSandbox.Api.Contracts.Schedule;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShiftRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}
