## Story 1: Stand up GraphQL & a TDD test harness

**As a** developer  
**I want** a secured GraphQL endpoint and an integration test harness  
**so that** I can add GraphQL features test-first and keep auth consistent.

### Acceptance Criteria
- `/graphql` is hosted in the API.  
- Dev-only IDE (Banana Cake Pop/Nitro) is enabled; disabled in non-dev builds.  
- Okta auth is enforced on GraphQL requests (unauthenticated calls get 401/403).  
- TIP: You can configure Nitro and Swagger to login and fetch a token from Okta.
- An integration test project can execute GraphQL operations against an in-memory server (`WebApplicationFactory`) using a test token (stubbed JWT).
- At least **3 failing tests** are written _before_ implementation (e.g., health query, unauthorized query, authorized query).

### Tests to write (red first)
- **Unauthorized request** to `/graphql` → 401/403.  
- **Authorized trivial query** (e.g., small `health` field (a bespoke dummy field used as a health check in a simple query)) → 200 OK.  
- **Schema snapshot**: introspection includes `Query`, `Mutation`, `Subscription`. (It’s basically a "schema smoke test" to confirm your GraphQL server is up and your core entry points are defined before you build anything else. You’re not testing real business logic here — you’re just proving the GraphQL schema skeleton is in place.)

### Hints / Guidance
- Packages: `HotChocolate.AspNetCore`, `HotChocolate.Data`, `HotChocolate.Subscriptions`.  
- Minimal Program.cs:
  ```csharp
  builder.Services
    .AddAuthentication(/* Okta */)
    .AddJwtBearer(/* ... */);
  builder.Services.AddAuthorization();

  builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType(d => d.Name("Query"))
    .AddProjections()
    .AddFiltering()
    .AddSorting();

  app.UseAuthentication();
  app.UseAuthorization();
  app.MapGraphQL("/graphql");
  ```
- In tests, DIY a JWT with the same audience/issuer as Okta (or use a test auth handler).  
- Gate the IDE:
  ```csharp
  if (app.Environment.IsDevelopment())
    app.MapBananaCakePop("/graphql/ui");
  ```

---

## Story 2: Communications List server pagination)

**As a** user  
**I want** a server-paginated list of communications  
**so that** list screens and reports can query efficiently.

### Acceptance Criteria
- Cursor paging (relay style) with `edges{ node{...} cursor }` and `pageInfo{ hasNextPage endCursor }`.  
- Filters work in combination (`type`, `status`).  

### Tests to write (red first)
- Page 1 (`first: 2`) returns 2 items + `hasNextPage=true`.  
- Page 2 using returned `endCursor` returns the next items.   
- Snapshot of result shape (be tolerant to non-breaking ordering of fields).

### Hints / Guidance
- If you did not implement pagination in your communications list, you'll need to find a solution for that first.
    - You need the solution to support 
- Use `HotChocolate.Data` projections/filtering/sorting to minimize custom code.  
- Build cursors from a stable key (e.g., composite of `lastUpdatedUtc` and `id`).  
- You could consider server-side defaults: `first` max (e.g., 50) to protect the API.  
- example query:
```
query {
  communications(first: 2, after: "YXJyYXljb25uZWN0aW9uOjE=") {
    edges {
      cursor
      node {
        id
        title
      }
    }
    pageInfo {
      hasNextPage
      endCursor
    }
  }
}
```