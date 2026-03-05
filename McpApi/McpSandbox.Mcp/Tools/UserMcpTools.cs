using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Users;
using McpSandbox.Mcp.Api;
using Microsoft.AspNetCore.JsonPatch;
using ModelContextProtocol.Server;
using System.ComponentModel;
using static McpSandbox.Mcp.Api.ApiResponseExtensions;

namespace McpSandbox.Mcp.Tools;

[McpServerToolType]
public sealed class UserMcpTools(IUsersApi usersApi)
{
    [McpServerTool, Description("List users.")]
    public Task<PagedResult<UserDto>> Users_List(
        string? search = null,
        int page = 1,
        int pageSize = 20)
        => usersApi.SearchAsync(search, page, pageSize).UnwrapAsync();

    [McpServerTool, Description("Get user by id.")]
    public Task<UserDto> Users_Get(string id)
        => usersApi.GetByIdAsync(Parse(id)).UnwrapAsync();

    [McpServerTool, Description("Create user.")]
    public Task<UserDto> Users_Create(CreateUserRequest request)
        => usersApi.CreateAsync(request).UnwrapAsync();

    [McpServerTool, Description("Update user.")]
    public Task<UserDto> Users_Update(string id, UpdateUserRequest request)
        => usersApi.UpdateAsync(Parse(id), request).UnwrapAsync();

    [McpServerTool, Description("Patch user.")]
    public Task<UserDto> Users_Patch(string id, JsonPatchDocument<PatchUserRequest> operations)
        => usersApi.PatchAsync(Parse(id), operations).UnwrapAsync();

    [McpServerTool, Description("Delete user.")]
    public async Task<string> Users_Delete(string id)
    {
        (await usersApi.DeleteAsync(Parse(id))).EnsureSuccess();
        return "Deleted";
    }

    private static Guid Parse(string id)
        => Guid.TryParse(id, out var g)
            ? g
            : throw new ArgumentException("Invalid UUID", nameof(id));
}