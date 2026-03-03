namespace McpSandbox.Mcp.Domain.Entities;

public class PromptVersion : BaseEntity
{
    public Guid PromptId { get; set; }

    public int VersionNumber { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    public Prompt Prompt { get; set; } = null!;
}
