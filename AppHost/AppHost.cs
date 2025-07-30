var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldata");

var webApi = builder.AddProject<Projects.WebApi>("webapi")
    .WithHttpHealthCheck("/health")
    .WithReference(sql)
    .WaitFor(sql);

builder.AddProject<Projects.BlazorServer>("blazorserver")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(webApi)
    .WaitFor(webApi);

builder.Build().Run();
