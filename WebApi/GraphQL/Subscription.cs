
namespace WebApi.GraphQL;

public class Subscription
{
    [Subscribe] public string DummySubscription([EventMessage] string message) => message; 
}
