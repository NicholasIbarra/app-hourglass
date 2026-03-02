var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("mssql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

var mcpdb = sql.AddDatabase("mcpdb");

var server = builder.AddProject<Projects.McpSandbox_Api>("server")
    .WithReference(mcpdb)
    .WaitFor(mcpdb)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.AddProject<Projects.McpSandbox_Mcp>("mcpsandbox-mcp");

builder.Build().Run();
