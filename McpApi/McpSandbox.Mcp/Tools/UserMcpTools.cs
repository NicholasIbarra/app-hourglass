using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpSandbox.Mcp.Tools;

[McpServerToolType]
public sealed class UserMcpTools
{
    [McpServerTool(Name = "random"), Description("gets a random number between two numbers.")]
    public static Task<int> GetRandomNumber(int min, int max, CancellationToken cancellationToken = default)
    {
        var random = new Random();
        int result = random.Next(min, max);
        return Task.FromResult(result);
    }
}