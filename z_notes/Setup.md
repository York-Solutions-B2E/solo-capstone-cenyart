```xml
// share secret id in .csproj
<UserSecretsId>aspire-shared-guid</UserSecretsId>
```

```zsh
dotnet user-secrets set "Okta:OktaDomain" ""
dotnet user-secrets set "Okta:ClientId" ""
dotnet user-secrets set "Okta:ClientSecret" ""
dotnet user-secrets set "Okta:AuthorizationServerId" "default"
dotnet user-secrets set "ConnectionStrings:sqldata" "Server=localhost,1433;Database=sqldata;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=true"

dotnet user-secrets list --project BlazorServer
dotnet user-secrets list --project WebApi
```

```zsh
// Create Shared Folder
dotnet new classlib --framework net8.0
dotnet add BlazorServer reference Shared
dotnet add WebApi reference Shared
dotnet sln add Shared
```

```zsh
// Add Packages WebApi
dotnet add package Okta.AspNetCore \
&& dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0\
&& dotnet add package Microsoft.EntityFrameworkCore.SqlServer \
&& dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package RabbitMQ.Client --project WebApi

// AppHost
dotnet add package Aspire.Hosting.SqlServer
dotnet ef migrations add InitialCreate --project WebApi

// BlazorServer
dotnet add package Okta.AspNetCore --project BlazorServer
dotnet add package RabbitMQ.Client --project BlazorServer

// RabitMQ
dotnet add package Aspire.Hosting.RabbitMQ --project AppHost
dotnet add package Aspire.RabbitMQ.Client --project WebApi
dotnet add package Aspire.RabbitMQ.Client --project BlazorServer
```

```bash
# Add initial migration
dotnet ef migrations add InitialCreate --project WebApi

# Update database
dotnet ef database update --project WebApi

# Add subsequent migrations (example)
dotnet ef migrations add AddCommunicationTypeStatuses
dotnet ef database update

# For production deployment
dotnet ef database update --connection "YourProductionConnectionString"
```

```bash
# Check that the volume exists
docker volume ls
# Remove the sqlserver volume
docker volume rm sqlserver

docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourStrongPassword123!' \
  -p 1433:1433 --name sqldata \
  -d mcr.microsoft.com/mssql/server:2022-latest

docker run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management

dotnet run --project AppHost
dotnet run --project WebApi
dotnet run --project BlazorServer

dotnet clean
dotnet build
```

[Cmd+Shift+P] [Developer: Reload Window]

// AppHost
dotnet tool install --global aspire.cli --prerelease
dotnet add package Aspire.Hosting.Docker --prerelease

aspire publish -o docker-compose-artifacts
docker compose up -d --build
