using McpSandbox.Api.Contracts.Chat;

namespace McpSandbox.Mcp.Domain.Entities;

public class ConversationMessage : BaseEntity
{
    public Guid ConversationId { get; set; }

    public MessageRole Role { get; set; }

    public string Content { get; set; } = string.Empty;

    public Conversation Conversation { get; set; } = null!;
}
