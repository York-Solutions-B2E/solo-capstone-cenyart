var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("docker-compose");

// Disable randomized ports so Aspire assigns static ports
builder.Configuration["DcpPublisher:RandomizePorts"] = "false";

// SQL Server
var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldata");

// RabbitMQ
var rabbitmq = builder.AddRabbitMQ("rabbit")
    .WithDataVolume();

// Web API project
var webApi = builder.AddProject<Projects.WebApi>("webapi")
    .WithHttpEndpoint(name: "webapi-https", port: 7157)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(sql)
    .WithReference(rabbitmq)
    .WaitFor(sql)
    .WaitFor(rabbitmq)
    .WithEnvironment("OKTA_DOMAIN", builder.Configuration["Okta:Domain"]);

// Blazor Server project
builder.AddProject<Projects.BlazorServer>("blazorserver")
    .WithHttpEndpoint(name: "blazorserver-https", port: 5001)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(webApi)
    .WithReference(rabbitmq)
    .WaitFor(webApi)
    .WaitFor(rabbitmq)
    .WithEnvironment("OKTA_DOMAIN", builder.Configuration["Okta:Domain"])
    .WithEnvironment("OKTA_CLIENT_ID", builder.Configuration["Okta:ClientId"])
    .WithEnvironment("OKTA_CLIENT_SECRET", builder.Configuration["Okta:ClientSecret"]);

builder.Build().Run();
