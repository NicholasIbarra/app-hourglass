using System.Text.Json.Serialization;

namespace McpSandbox.Api.Contracts.Chat;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageRole
{
    System = 0,
    User = 1,
    Assistant = 2
}
