```zsh
// projects
dotnet new webapi -n WebApi --framework net8.0
dotnet new blazor -n BlazorServer --framework net8.0
dotnet new aspire-apphost -o AppHost --framework net8.0
dotnet new aspire-servicedefaults -o ServiceDefaults --framework net8.0
```

```zsh
// solution in root
dotnet new sln
dotnet sln add WebApi
dotnet sln add BlazorServer
dotnet sln add AppHost
dotnet sln add ServiceDefaults
```

```zsh
// AppHost ref projects
dotnet add AppHost reference WebApi
dotnet add AppHost reference BlazorServer

// projects ref ServiceDefault
dotnet add WebApi reference ServiceDefaults
dotnet add BlazorServer reference ServiceDefaults

// add > WebApi.Program > BlazorServer.Program
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
```

```csharp
// AppHost.cs
var builder = DistributedApplication.CreateBuilder(args);

var webApi = builder.AddProject<Projects.WebApi>("webapi")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.BlazorServer>("blazorserver")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(webApi)
    .WaitFor(webApi);

builder.Build().Run();
```

```json
// WebApi.appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "WebApiEndpoint": "http://webapi",
  "WebApiEndpointHttps": "https://webapi"
}
```

```zsh
dotnet run --project AppHost
dotnet build WebApi
```
