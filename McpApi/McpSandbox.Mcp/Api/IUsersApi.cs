using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Users;
using Microsoft.AspNetCore.JsonPatch;
using Refit;

namespace McpSandbox.Mcp.Api;

public interface IUsersApi
{
    [Get("/api/users/{id}")]
    Task<IApiResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/users")]
    Task<IApiResponse<PagedResult<UserDto>>> SearchAsync(
        [Query] string? search = null,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);

    [Post("/api/users")]
    Task<IApiResponse<UserDto>> CreateAsync([Body] CreateUserRequest request, CancellationToken cancellationToken = default);

    [Put("/api/users/{id}")]
    Task<IApiResponse<UserDto>> UpdateAsync(Guid id, [Body] UpdateUserRequest request, CancellationToken cancellationToken = default);

    [Patch("/api/Users/{id}")]
    Task<IApiResponse<UserDto>> PatchAsync(Guid id, [Body] JsonPatchDocument<PatchUserRequest> patch, CancellationToken cancellationToken = default);

    [Delete("/api/users/{id}")]
    Task<IApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
