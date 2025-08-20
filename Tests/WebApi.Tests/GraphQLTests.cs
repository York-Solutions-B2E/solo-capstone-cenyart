using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace WebApi.Tests;

[TestFixture]
public class GraphQLTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Unauthorized_query_returns_401_or_403()
    {
        var response = await _client.PostAsJsonAsync("/graphql", new { query = "{ health }" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task Authorized_health_query_returns_200()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "dummy-test-token");

        var response = await _client.PostAsJsonAsync("/graphql", new { query = "{ health }" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ok");
    }

    [Test]
    public async Task Schema_introspection_includes_root_types()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "dummy-test-token");

        var introspectionQuery = new
        {
            query = @"{
            __schema {
                queryType { name }
                mutationType { name }
                subscriptionType { name }
            }
        }"
        };

        var response = await _client.PostAsJsonAsync("/graphql", introspectionQuery);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("Query");
        json.Should().Contain("Mutation");
        json.Should().Contain("Subscription");
    }
}
