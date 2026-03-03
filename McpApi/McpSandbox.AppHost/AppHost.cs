var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("mssql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .PublishAsContainer();

var mcpdb = sql.AddDatabase("mcpdb");
var chatdb = sql.AddDatabase("chatdb");

var server = builder.AddProject<Projects.McpSandbox_Api>("server")
    .WithReference(mcpdb)
    .WaitFor(mcpdb)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

var mcp = builder.AddProject<Projects.McpSandbox_Mcp>("mcpsandbox-mcp")
    .WithReference(server)
    .WaitFor(server)
    .WithReference(chatdb)
    .WaitFor(chatdb);

webfrontend
    .WithReference(mcp)
    .WaitFor(mcp);

builder.Build().Run();
