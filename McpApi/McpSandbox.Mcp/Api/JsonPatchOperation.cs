using System.Text.Json.Serialization;

namespace McpSandbox.Mcp.Api;

public sealed record JsonPatchOperation(
    [property: JsonPropertyName("op")] string Op,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("value")] object? Value = null);
