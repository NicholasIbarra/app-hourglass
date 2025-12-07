var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .AddDatabase("TestDb");

builder.AddProject<Projects.Api>("api")
    .WithReference(sql)
    .WaitFor(sql);

builder.Build().Run();
