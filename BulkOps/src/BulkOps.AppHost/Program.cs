var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = sql.AddDatabase("bulkopsdb");

builder.AddProject<Projects.BulkOps_Api>("bulkops-api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
