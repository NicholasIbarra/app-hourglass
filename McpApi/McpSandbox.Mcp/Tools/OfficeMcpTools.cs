using McpSandbox.Api.Contracts.Offices;
using McpSandbox.Mcp.Api;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using static McpSandbox.Mcp.Api.ApiResponseExtensions;

namespace McpSandbox.Mcp.Tools;

[McpServerToolType]
public sealed class OfficeMcpTools(IOfficesApi officesApi)
{
    [McpServerTool(Name = "get_office"), Description("Gets an office by its unique identifier.")]
    public async Task<string> GetOffice(Guid id, CancellationToken cancellationToken = default)
    {
        var office = (await officesApi.GetByIdAsync(id, cancellationToken)).EnsureSuccess();
        return JsonSerializer.Serialize(office);
    }

    [McpServerTool(Name = "search_offices"), Description("Searches for offices by name, code, phone number, city, state/province, or country. Returns a paged list of offices.")]
    public async Task<string> SearchOffices(
        [Description("Optional search term to filter offices by name, code, phone, city, state, or country.")] string? search = null,
        [Description("Page number (1-based). Defaults to 1.")] int page = 1,
        [Description("Number of results per page (1-100). Defaults to 20.")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = (await officesApi.SearchAsync(search, page, pageSize, cancellationToken)).EnsureSuccess();
        return JsonSerializer.Serialize(result);
    }

    [McpServerTool(Name = "create_office"), Description("Creates a new office.")]
    public async Task<string> CreateOffice(CreateOfficeRequest request, CancellationToken cancellationToken = default)
    {
        var office = (await officesApi.CreateAsync(request, cancellationToken)).EnsureSuccess();
        return JsonSerializer.Serialize(office);
    }

    [McpServerTool(Name = "update_office"), Description("Updates an existing office by its unique identifier.")]
    public async Task<string> UpdateOffice(
        [Description("The unique identifier of the office to update.")] Guid id,
        UpdateOfficeRequest request,
        CancellationToken cancellationToken = default)
    {
        var office = (await officesApi.UpdateAsync(id, request, cancellationToken)).EnsureSuccess();
        return JsonSerializer.Serialize(office);
    }

    [McpServerTool(Name = "delete_office"), Description("Deletes an office by its unique identifier.")]
    public async Task<string> DeleteOffice(
        [Description("The unique identifier of the office to delete.")] Guid id,
        CancellationToken cancellationToken = default)
    {
        (await officesApi.DeleteAsync(id, cancellationToken)).EnsureSuccess();
        return JsonSerializer.Serialize(new { message = "Office deleted successfully.", id });
    }
}
