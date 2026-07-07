namespace McpSandbox.Mcp.Services.Chat;

public sealed class ChatAgentOptions
{
    public const string SectionName = "ChatAgent";

    public int MaxToolIterations { get; set; } = 8;
}
