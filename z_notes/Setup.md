```xml
// share secret id in .csproj
<UserSecretsId>aspire-shared-guid</UserSecretsId>
```

```zsh
dotnet user-secrets set "Okta:OktaDomain" ""
dotnet user-secrets set "Okta:ClientId" ""
dotnet user-secrets set "Okta:ClientSecret" ""
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
dotnet add package Okta.AspNetCore --project WebApi
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0 --project WebApi
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --project WebApi
dotnet add package Microsoft.EntityFrameworkCore.Design --project WebApi
dotnet add package RabbitMQ.Client --project WebApi

// BlazorServer
dotnet add package Okta.AspNetCore --project BlazorServer
dotnet add package RabbitMQ.Client --project BlazorServer
```

```bash
# Add initial migration
dotnet ef migrations add InitialCreate --project WebApi

# Update database
dotnet ef database update --project WebApi
```

```bash
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourStrongPassword123!' \
  -p 1433:1433 --name sqldatalocal \
  -d mcr.microsoft.com/mssql/server:2022-latest

docker run -d \
  --name rabbitmqlocal \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management

dotnet run --project WebApi
dotnet run --project BlazorServer

docker-compose down
docker-compose up --build
https://localhost:5001/
https://localhost:7157/swagger

dotnet clean
dotnet build
```

```bash
dotnet add package HotChocolate.AspNetCore --project WebApi
dotnet add package HotChocolate.Data --project WebApi
dotnet add package HotChocolate.Subscriptions --project WebApi
dotnet add package HotChocolate.AspNetCore.Authorization --project WebApi

https://localhost:7157/graphql/

dotnet run --launch-profile "WebApi (Development)" --project WebApi
dotnet run --launch-profile "WebApi (Production)" --project WebApi
```

```bash
mkdir tests
cd tests
dotnet new nunit -n WebApi.Tests
dotnet add WebApi.Tests reference ../WebApi/WebApi.csproj
cd ..
dotnet sln add Tests/WebApi.Tests
dotnet add Tests/WebApi.Tests package Microsoft.AspNetCore.Mvc.Testing --version 8.0.19
dotnet add Tests/WebApi.Tests package FluentAssertions
dotnet add Tests/WebApi.Tests package Microsoft.EntityFrameworkCore.InMemory
dotnet add Tests/WebApi.Tests package Snapshooter.NUnit

dotnet test
```
