
## Story 1 — Dual GraphQL schemas: **User** vs **Admin**

**As an** administrator  
**I want** a separate **Admin GraphQL endpoint** with a broader schema than the **User** endpoint  
**so that** least‑privilege is enforced and sensitive fields/operations are not even visible to non‑admins.

### Acceptance Criteria
- Two GraphQL endpoints:
  - `/graphql` → **User schema** (read‑only + limited fields)
  - `/graphql/admin` → **Admin schema** (adds admin-only fields/mutations)
- Access control:
  - `/graphql/admin` requires `role=Admin` (or equivalent) — non‑admins see an auth error in the GraphQL response.
  - `/graphql` is available to any authenticated user.
- Schema separation:
  - Admin-only types/fields/mutations do **not** appear in User schema introspection.
  - In non‑dev environments, disable Admin schema introspection (keep in dev).
- Documentation: README shows how to authenticate to each endpoint (via Okta or a stubbed token for tests).

### Tests to write (red first)
- Calling `/graphql/admin` **without** `role=Admin` returns a GraphQL auth error; with `role=Admin` succeeds.
- Introspection of `/graphql` does **not** include admin-only types/fields.
- A field only defined in the Admin schema is **not callable** on `/graphql` (expect “field not found”).
- Snapshot both schemas to guard against accidental exposure.

### Hints / Guidance
- Use **Hot Chocolate named schemas**:
  ```csharp
  services.AddGraphQLServer("user")
          .AddQueryType<UserQuery>();

  services.AddGraphQLServer("admin")
          .AddQueryType<AdminQuery>();
          .AddMutationType<AdminMutation>();

  app.MapGraphQL("/graphql", schemaName: "user");
  app.MapGraphQL("/graphql/admin", schemaName: "admin")
     .RequireAuthorization("Admin"); // policy maps to role=Admin
  ```
- Put `[Authorize(Roles = "Admin")]` on admin resolvers **and** protect the `/graphql/admin` endpoint.
- In prod/staging, unmap the IDE for admin or disable introspection; keep it only in dev.

---

## Story 2 — Fine‑grained access with **Okta custom claims**

**As a** product owner  
**I want** Okta to issue a claim that lists which **communication types** a user may access  
**so that** users only see data relevant to their department (e.g., ID Cards vs EOBs).

### Acceptance Criteria
- Okta Authorization Server issues a JSON array claim, e.g.:
  ```json
  "allowedTypes": ["ID_CARD", "EOB"]
  ```
- GraphQL resolvers **filter** results based on `allowedTypes`:
  - List queries return only rows where `TypeCode ∈ allowedTypes`.
  - Single‑item queries return **null/forbidden** if the item’s `TypeCode` is not allowed.
- Admin users bypass filtering (see Story 1).
- Missing claim defaults to **deny** (return nothing).
- README documents how the claim is provisioned in Okta and how to test.

### Tests to write (red first)
- Token with `allowedTypes=["ID_CARD"]`:  
  - `communications` returns only `TypeCode="ID_CARD"`.  
  - `communication(id: EOB-item)` returns null/forbidden.
- Token with `allowedTypes=["EOB","ID_CARD"]`: both types are visible.
- Token **without** claim: no items returned (deny by default).
- Admin token: all types visible (bypass).
- Unit tests for the claim-parsing helper (handles JSON array vs CSV, absent claim, casing).

### Hints / Guidance
- **Okta**: Add a **custom claim** on the Authorization Server (e.g., name `allowedTypes`, type JSON).  
  Populate via an Okta rule or by mapping groups (e.g., `comm-idcards`, `comm-eob`) to values.
- **Resolvers**:
  ```csharp
  static string[] GetAllowedTypes(ClaimsPrincipal user) =>
      user.FindFirst("allowedTypes") is { } c
        ? System.Text.Json.JsonSerializer.Deserialize<string[]>(c.Value) ?? Array.Empty<string>()
        : Array.Empty<string>();

  // In a query resolver using EF:
  var allowed = GetAllowedTypes(context.User);
  if (!user.IsInRole("Admin"))
      query = query.Where(c => allowed.Contains(c.TypeCode)); // becomes SQL IN (...)
  ```
- Apply the same rule to **mutations** (e.g., `publishEvent`) to prevent writing disallowed types.

---

## Story 3 — **Refresh tokens & silent renew** for Blazor Server

**As a** user  
**I want** my access token to refresh automatically before it expires  
**so that** I don’t get kicked out or have to re-login during normal use.

### Acceptance Criteria
- Okta app configured to issue **Refresh Tokens** (enable `offline_access` scope; rotation recommended).
- Blazor Server uses OIDC with `SaveTokens=true`; access/refresh tokens are stored securely (encrypted auth cookie).
- Automatic refresh occurs **before** `access_token` expiry (e.g., 2 minutes early).  
  On refresh failure (revoked/expired refresh token), the user is redirected to interactive login.
- API calls made from the server include a **fresh** `Authorization: Bearer` token (no 401s due to expiry under normal conditions).
- README documents token lifetimes, scopes, and local dev caveats.

### Tests to write (red first)
- **Unit**: Token service refreshes when `expires_at - now < threshold`, returns new tokens, and updates the store.
- **Integration (dev/test)**: Simulate an expiring token (short lifetime) → make two API calls separated by “expiry window”; second call succeeds without re-login.
- **Failure path**: Force refresh to fail → user is sent to login.
- **Security**: Refresh token is **not** logged and is stored only in the server-side auth ticket.

### Hints / Guidance
- Okta: enable **Refresh Token** grant for your OIDC app and add `offline_access` to requested scopes.
- ASP.NET Core:
  ```csharp
  services.AddAuthentication(options => {
      options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
  })
  .AddCookie()
  .AddOpenIdConnect("Okta", options => {
      options.Authority = "{yourOktaIssuer}";
      options.ClientId = "{clientId}";
      options.ClientSecret = "{secret}";           // if using confidential app
      options.ResponseType = "code";
      options.SaveTokens = true;                   // stores tokens in auth properties (encrypted)
      options.Scope.Add("openid");
      options.Scope.Add("profile");
      options.Scope.Add("offline_access");         // get refresh token
  });
  ```
- Add a **token management** service (e.g., using `IdentityModel.AspNetCore`):
  ```csharp
  services.AddAccessTokenManagement(o => {
      o.User.RefreshBeforeExpiration = TimeSpan.FromMinutes(2);
  });
  services.AddHttpClient("api")
          .AddUserAccessTokenHandler(); // injects and refreshes tokens automatically
  ```
- For GraphQL clients from Blazor, use an `HttpClient` configured with `AddUserAccessTokenHandler()` so calls always carry a fresh token.
- Blazor Server is **server-rendered**: you refresh via the server’s back-channel (token endpoint), not a hidden iframe like SPAs.

---

### Okta foundation/knowledge check
- How does Okta handle authentication?
- What’s the difference between OIDC and OAuth 2.0?
- How are roles represented in Okta?
- How do applications consume role/permission information from an Okta-issued token?
- What’s the difference between an ID token, access token, and refresh token?
- How do we validate an access token server-side?
- How does Okta enforce MFA?
- What policies or rules can be set at the org level to strengthen authentication?
- What steps are required to onboard a new application into Okta for SSO?
- How does Okta handle redirect URIs and logout URIs for applications?
- You'll be integrating an Okta solution into legacy apps such as DOCs, LofaC, and ZAPP/ZPASS.
- How would Okta federate identities across multiple systems while maintaining a single login?
- What are some challenges of introducing Okta into a system that currently has monolithic authentication models?

### Architecture Questions
- The lifecycle docs highlight millions of transactions (e.g., 13M cards Dec/Jan) and 5 9s availability. What Okta architecture considerations (e.g., rate limits, caching, session scaling) need to be accounted for?
- The docs emphasize HIPAA/PCI compliance, PHI-free environments, and auditability. How does Okta support compliance requirements in healthcare/financial domains?
- What logging and audit capabilities does Okta provide to support security audits?
- Since different payer clients may have different configurations, how could Okta handle multi-tenancy or delegated administration?
- The project will be replacing legacy auth in DOCs/LofaC with Okta. What approaches can be used to migrate existing users into Okta without forcing immediate password resets?
- How should we plan for iterative rollout vs big bang cutover?
---