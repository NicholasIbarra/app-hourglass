var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql", port: 61989)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase(name: "CadenceDbDev");

builder.AddProject<Projects.Cadence_Api>("api")
    .WithHttpsEndpoint(5050, name: "api")
    .WithReference(sql)
    .WaitFor(sql);

builder.Build().Run();
