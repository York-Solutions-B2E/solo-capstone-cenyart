using Microsoft.AspNetCore.Authorization;

namespace WebApi.GraphQL.User;

[ExtendObjectType("Mutation")]
[Authorize(Roles = "User")]
public class UserMutation
{
    public string DummyMutation() => "ok";
}
