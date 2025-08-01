var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldata");

var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithDataVolume();

var webApi = builder.AddProject<Projects.WebApi>("webapi")
    .WithHttpHealthCheck("/health")
    .WithReference(sql)
    .WithReference(rabbitmq)
    .WaitFor(sql)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.BlazorServer>("blazorserver")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(webApi)
    .WaitFor(webApi);

builder.Build().Run();
