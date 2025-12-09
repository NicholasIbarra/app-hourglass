var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("TestDb");

builder.AddProject<Projects.Cadence_Api>("api")
    .WithHttpsEndpoint(5050, name: "api")
    .WithReference(sql)
    .WaitFor(sql);

builder.Build().Run();
