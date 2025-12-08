var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("TestDb");

builder.AddProject<Projects.Api>("api")
    .WithReference(sql)
    .WaitFor(sql);

builder.Build().Run();
