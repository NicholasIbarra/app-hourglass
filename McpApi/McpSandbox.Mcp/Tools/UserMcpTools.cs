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
    //[McpServerTool(Name = "get_user"), Description("Gets a user by their unique identifier.")]
    //public async Task<string> GetUser(Guid id, CancellationToken cancellationToken = default)
    //{
    //    var user = await usersApi.GetByIdAsync(id, cancellationToken);
    //    return JsonSerializer.Serialize(user);
    //}

    //[McpServerTool(Name = "search_users"), Description("Searches for users by name, email, or phone number. Returns a paged list of users.")]
    //public async Task<string> SearchUsers(
    //    [Description("Optional search term to filter users by name, email, or phone number.")] string? search = null,
    //    [Description("Page number (1-based). Defaults to 1.")] int page = 1,
    //    [Description("Number of results per page (1-100). Defaults to 20.")] int pageSize = 20,
    //    CancellationToken cancellationToken = default)
    //{
    //    var result = await usersApi.SearchAsync(search, page, pageSize, cancellationToken);
    //    return JsonSerializer.Serialize(result);
    //}

    //[McpServerTool(Name = "create_user"), Description("Creates a new user.")]
    //public async Task<string> CreateUser(CreateUserRequest request, CancellationToken cancellationToken = default)
    //{
    //    var user = await usersApi.CreateAsync(request, cancellationToken);
    //    return JsonSerializer.Serialize(user);
    //}

    //[McpServerTool(Name = "update_user"), Description("Updates an existing user by their unique identifier.")]
    //public async Task<string> UpdateUser(
    //    [Description("The unique identifier of the user to update.")] Guid id,
    //    UpdateUserRequest request,
    //    CancellationToken cancellationToken = default)
    //{
    //    var user = await usersApi.UpdateAsync(id, request, cancellationToken);
    //    return JsonSerializer.Serialize(user);
    //}

    //[McpServerTool(Name = "patch_user"), Description("Partially updates a user by their unique identifier. Only fields included in the request are changed; omitted fields (null) remain unchanged.")]
    //public async Task<string> PatchUser(
    //    [Description("The unique identifier of the user to patch.")] Guid id,
    //    [Description("Fields to update. Only include fields you want to change; null fields are ignored.")] PatchUserRequest request,
    //    CancellationToken cancellationToken = default)
    //{
    //    var operations = new List<JsonPatchOperation>();

    //    if (request.Name is not null) operations.Add(new("replace", "/name", request.Name));
    //    if (request.Email is not null) operations.Add(new("replace", "/email", request.Email));
    //    if (request.Type is not null) operations.Add(new("replace", "/type", request.Type));
    //    if (request.IsActive is not null) operations.Add(new("replace", "/isActive", request.IsActive));
    //    if (request.PhoneNumber is not null) operations.Add(new("replace", "/phoneNumber", request.PhoneNumber));
    //    if (request.TimeZone is not null) operations.Add(new("replace", "/timeZone", request.TimeZone));
    //    if (request.Locale is not null) operations.Add(new("replace", "/locale", request.Locale));
    //    if (request.AvatarUrl is not null) operations.Add(new("replace", "/avatarUrl", request.AvatarUrl));
    //    if (request.LastLoginAt is not null) operations.Add(new("replace", "/lastLoginAt", request.LastLoginAt));
    //    if (request.OfficeIds is not null) operations.Add(new("replace", "/officeIds", request.OfficeIds));

    //    var user = await usersApi.PatchAsync(id, operations, cancellationToken);
    //    return JsonSerializer.Serialize(user);
    //}

    //[McpServerTool(Name = "delete_user"), Description("Deletes a user by their unique identifier.")]
    //public async Task<string> DeleteUser(
    //    [Description("The unique identifier of the user to delete.")] Guid id,
    //    CancellationToken cancellationToken = default)
    //{
    //    await usersApi.DeleteAsync(id, cancellationToken);
    //    return JsonSerializer.Serialize(new { message = "User deleted successfully.", id });
    //}

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