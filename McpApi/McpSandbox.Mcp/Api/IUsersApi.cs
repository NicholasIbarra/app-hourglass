using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Users;
using Refit;

namespace McpSandbox.Mcp.Api;

public interface IUsersApi
{
    [Get("/api/users/{id}")]
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/api/users")]
    Task<PagedResult<UserDto>> SearchAsync(
        [Query] string? search = null,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);
}
