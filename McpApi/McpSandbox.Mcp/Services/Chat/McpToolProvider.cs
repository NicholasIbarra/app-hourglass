using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace McpSandbox.Mcp.Services.Chat;

public interface IMcpToolProvider
{
    Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default);
}

public sealed class McpToolProvider : IMcpToolProvider, IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<McpToolProvider> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private McpClient? _mcpClient;
    private IReadOnlyList<AITool>? _tools;

    public McpToolProvider(IServiceProvider serviceProvider, ILogger<McpToolProvider> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        if (_tools is not null)
            return _tools;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_tools is not null)
                return _tools;

            var server = _serviceProvider.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();

            var baseAddress = addresses?.Addresses.FirstOrDefault()
                ?? throw new InvalidOperationException("No server address available.");

            var mcpEndpoint = new Uri(new Uri(baseAddress), "/mcp");
            _logger.LogInformation("Connecting MCP client to {Endpoint}", mcpEndpoint);

            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = mcpEndpoint
            });

            _mcpClient = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);

            var tools = await _mcpClient.ListToolsAsync(cancellationToken: cancellationToken);
            _tools = tools.ToList();

            _logger.LogInformation("MCP client connected. {Count} tools available.", _tools.Count);
            return _tools;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_mcpClient is not null)
        {
            await _mcpClient.DisposeAsync();
            _mcpClient = null;
        }

        _initLock.Dispose();
    }
}
