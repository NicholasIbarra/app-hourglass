using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Unavailabilities;
using McpSandbox.Mcp.Api;
using ModelContextProtocol.Server;
using System.ComponentModel;
using static McpSandbox.Mcp.Api.ApiResponseExtensions;

namespace McpSandbox.Mcp.Tools;

[McpServerToolType]
public sealed class UnavailabilityMcpTools(IUnavailabilitiesApi unavailabilitiesApi)
{
    [McpServerTool, Description("List unavailabilities. Filter by user, date range, search text, or active status.")]
    public Task<PagedResult<UnavailabilityDto>> Unavailabilities_List(
        [Description("Filter by user ID.")] string? userId = null,
        [Description("Search by reason or notes.")] string? search = null,
        [Description("Filter to unavailabilities overlapping on or after this date.")] DateTime? from = null,
        [Description("Filter to unavailabilities overlapping on or before this date.")] DateTime? to = null,
        [Description("Filter by active status.")] bool? isActive = null,
        int page = 1,
        int pageSize = 20)
        => unavailabilitiesApi.SearchAsync(
            ParseOptional(userId),
            search,
            from,
            to,
            isActive,
            page,
            pageSize).UnwrapAsync();

    [McpServerTool, Description("Get unavailability by id.")]
    public Task<UnavailabilityDto> Unavailabilities_Get(string id)
        => unavailabilitiesApi.GetByIdAsync(Parse(id)).UnwrapAsync();

    [McpServerTool, Description("Create unavailability.")]
    public Task<UnavailabilityDto> Unavailabilities_Create(CreateUnavailabilityRequest request)
        => unavailabilitiesApi.CreateAsync(request).UnwrapAsync();

    [McpServerTool, Description("Update unavailability.")]
    public Task<UnavailabilityDto> Unavailabilities_Update(string id, UpdateUnavailabilityRequest request)
        => unavailabilitiesApi.UpdateAsync(Parse(id), request).UnwrapAsync();

    [McpServerTool, Description("Delete unavailability.")]
    public async Task<string> Unavailabilities_Delete(string id)
    {
        (await unavailabilitiesApi.DeleteAsync(Parse(id))).EnsureSuccess();
        return "Deleted";
    }

    private static Guid Parse(string id)
        => Guid.TryParse(id, out var g)
            ? g
            : throw new ArgumentException("Invalid UUID", nameof(id));

    private static Guid? ParseOptional(string? id)
        => string.IsNullOrWhiteSpace(id) ? null
            : Guid.TryParse(id, out var g) ? g
            : throw new ArgumentException("Invalid UUID", nameof(id));
}
