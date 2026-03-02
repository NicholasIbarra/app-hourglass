using McpSandbox.Api.Contracts.Users;
using McpSandbox.Mcp.Api;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace McpSandbox.Mcp.Tools;

[McpServerToolType]
public sealed class UserMcpTools(IUsersApi usersApi)
{
    [McpServerTool(Name = "get_user"), Description("Gets a user by their unique identifier.")]
    public async Task<string> GetUser(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await usersApi.GetByIdAsync(id, cancellationToken);
        return JsonSerializer.Serialize(user);
    }

    [McpServerTool(Name = "search_users"), Description("Searches for users by name, email, or phone number. Returns a paged list of users.")]
    public async Task<string> SearchUsers(
        [Description("Optional search term to filter users by name, email, or phone number.")] string? search = null,
        [Description("Page number (1-based). Defaults to 1.")] int page = 1,
        [Description("Number of results per page (1-100). Defaults to 20.")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await usersApi.SearchAsync(search, page, pageSize, cancellationToken);
        return JsonSerializer.Serialize(result);
    }
}