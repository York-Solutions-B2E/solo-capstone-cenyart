```xml
// .csproj
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






