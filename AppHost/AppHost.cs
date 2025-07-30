var builder = DistributedApplication.CreateBuilder(args);

var webApi = builder.AddProject<Projects.WebApi>("webapi")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.BlazorServer>("blazorserver")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(webApi)
    .WaitFor(webApi);

builder.Build().Run();
