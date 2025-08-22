
using Microsoft.AspNetCore.Authorization;

namespace WebApi.GraphQL.Admin;

[ExtendObjectType("Query")]
[Authorize(Roles = "Admin")]
public class AdminQuery
{
    public string AdminOnlyField() => "secret";
}
