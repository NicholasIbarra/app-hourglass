namespace McpSandbox.Mcp.Services.Chat;

public interface IChatAgentService
{
    IAsyncEnumerable<string> StreamChatResponseAsync(
        Guid conversationId,
        string userMessage,
        Guid? promptVersionId,
        CancellationToken cancellationToken = default);
}
