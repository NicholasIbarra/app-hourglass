namespace McpSandbox.Mcp.Domain.Entities;

public class Prompt : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<PromptVersion> Versions { get; set; } = new List<PromptVersion>();
}
