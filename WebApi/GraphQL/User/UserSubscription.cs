using Microsoft.AspNetCore.Authorization;

namespace WebApi.GraphQL.User;

[ExtendObjectType("Subscription")]
[Authorize(Roles = "User")]
public class UserSubscription
{
    [Subscribe] public string DummySubscription([EventMessage] string message) => message;
}
