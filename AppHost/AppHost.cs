var builder = DistributedApplication.CreateBuilder(args);

var compose = builder.AddDockerComposeEnvironment("compose")
                    .WithDashboard(dashboard => dashboard.WithHostPort(8080));

builder.Configuration["DcpPublisher:RandomizePorts"] = "false";

// SQL Server
var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldata");

// RabbitMQ
var rabbitmq = builder.AddRabbitMQ("rabbit")
    .WithDataVolume();

var oktaDomainParam = builder.AddParameter("okta-domain");
var oktaApi = builder.AddExternalService("okta-api", oktaDomainParam);

// Web API
var webApi = builder.AddProject<Projects.WebApi>("webapi")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(sql)
    .WithReference(rabbitmq)
    .WithReference(oktaApi)
    .WaitFor(sql)
    .WaitFor(rabbitmq)
    .WithEnvironment("OKTA_DOMAIN", "${OKTA_DOMAIN}");

// Blazor Server
builder.AddProject<Projects.BlazorServer>("blazorserver")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(webApi)
    .WithReference(rabbitmq)
    .WithReference(oktaApi)
    .WaitFor(webApi)
    .WaitFor(rabbitmq)
    .WithEnvironment("OKTA_DOMAIN", "${OKTA_DOMAIN}")
    .WithEnvironment("OKTA_CLIENT_ID", "${OKTA_CLIENT_ID}")
    .WithEnvironment("OKTA_CLIENT_SECRET", "${OKTA_CLIENT_SECRET}");

builder.Build().Run();
