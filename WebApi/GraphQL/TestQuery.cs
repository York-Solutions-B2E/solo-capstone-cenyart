
namespace WebApi.GraphQL;

public class Query { public string Health() => "ok"; }
public class Mutation { public string DummyMutation() => "ok"; }
public class Subscription { [Subscribe] public string DummySubscription([EventMessage] string message) => message; }


