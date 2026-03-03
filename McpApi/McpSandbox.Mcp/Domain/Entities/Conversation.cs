using McpSandbox.Api.Contracts.Chat;

namespace McpSandbox.Mcp.Domain.Entities;

public class Conversation : BaseEntity
{
    public string? Title { get; set; }

    public Guid? UserId { get; set; }

    public ConversationStatus Status { get; set; } = ConversationStatus.Active;

    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}
