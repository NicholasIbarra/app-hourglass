using OpenAI.Chat;

namespace McpSandbox.Mcp.Services.Chat;

public interface IMcpToolClient
{
    IReadOnlyList<ChatTool> GetToolDefinitions();

    Task<string> InvokeAsync(string toolName, string? toolArgumentsJson, CancellationToken cancellationToken = default);
}
