using System.Text.Json.Serialization;

namespace McpSandbox.Api.Contracts.Chat;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConversationStatus
{
    Active = 0,
    Archived = 1
}
