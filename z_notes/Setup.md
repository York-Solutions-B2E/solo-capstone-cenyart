```xml
// share secret id in .csproj
<UserSecretsId>aspire-shared-guid</UserSecretsId>
```

```zsh
dotnet user-secrets set "Okta:OktaDomain" ""
dotnet user-secrets set "Okta:ClientId" ""
dotnet user-secrets set "Okta:ClientSecret" ""
dotnet user-secrets set "Okta:AuthorizationServerId" "default"

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
// Add Packages
dotnet add package Okta.AspNetCore \
&& dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0\
&& dotnet add package Microsoft.EntityFrameworkCore.SqlServer \
&& dotnet add package Microsoft.EntityFrameworkCore.Design
```
