using System.Text.Json.Serialization;

namespace McpSandbox.Api.Contracts.Users;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserType
{
    Staff = 0,
    Provider = 1,
    NonScheduling = 2
}
