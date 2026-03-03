using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Availabilities;
using McpSandbox.Mcp.Api;
using ModelContextProtocol.Server;
using System.ComponentModel;
using static McpSandbox.Mcp.Api.ApiResponseExtensions;

namespace McpSandbox.Mcp.Tools;

[McpServerToolType]
public sealed class AvailabilityMcpTools(IAvailabilitiesApi availabilitiesApi)
{
    [McpServerTool, Description("List availabilities. Filter by user, office, or active status.")]
    public Task<PagedResult<AvailabilityDto>> Availabilities_List(
        [Description("Filter by user ID.")] string? userId = null,
        [Description("Filter by office ID.")] string? officeId = null,
        [Description("Filter by active status.")] bool? isActive = null,
        int page = 1,
        int pageSize = 20)
        => availabilitiesApi.SearchAsync(
            ParseOptional(userId),
            ParseOptional(officeId),
            isActive,
            page,
            pageSize).UnwrapAsync();

    [McpServerTool, Description("Get availability by id.")]
    public Task<AvailabilityDto> Availabilities_Get(string id)
        => availabilitiesApi.GetByIdAsync(Parse(id)).UnwrapAsync();

    [McpServerTool, Description("Create availability.")]
    public Task<AvailabilityDto> Availabilities_Create(CreateAvailabilityRequest request)
        => availabilitiesApi.CreateAsync(request).UnwrapAsync();

    [McpServerTool, Description("Update availability.")]
    public Task<AvailabilityDto> Availabilities_Update(string id, UpdateAvailabilityRequest request)
        => availabilitiesApi.UpdateAsync(Parse(id), request).UnwrapAsync();

    [McpServerTool, Description("Delete availability.")]
    public async Task<string> Availabilities_Delete(string id)
    {
        (await availabilitiesApi.DeleteAsync(Parse(id))).EnsureSuccess();
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
