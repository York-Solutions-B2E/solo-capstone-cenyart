using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Snapshooter.NUnit;

namespace Tests.GraphQL;

[TestFixture]
public class AdminSchemaTests
{
    private ConfigureApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new ConfigureApplicationFactory();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Test]
    public async Task AdminEndpoint_WithoutAdminRole_ReturnsAuthError()
    {
        var response = await _client.PostAsJsonAsync("/graphql/admin", new { query = "{ adminOnlyField }" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task AdminEndpoint_WithAdminRole_Succeeds()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "dummy-admin-token");

        var response = await _client.PostAsJsonAsync("/graphql/admin", new { query = "{ adminOnlyField }" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        Snapshot.Match(
            body,
            opts => opts.IgnoreField("$.extensions")
        );
    }

    [Test]
    public async Task Introspection_UserSchema_DoesNotIncludeAdminFields()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "dummy-user-token");

        var introspectionQuery = new
        {
            query = @"{
                __type(name: ""Query"") {
                    fields {
                        name
                    }
                }
            }"
        };

        var response = await _client.PostAsJsonAsync("/graphql", introspectionQuery);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        Snapshot.Match(
            body,
            opts => opts.IgnoreField("$.extensions")
        );

        body.Should().NotContain("adminOnlyField",
            "User schema introspection should not expose admin-only fields.");
    }

    [Test]
    public async Task CallingAdminField_OnUserSchema_ReturnsFieldDoesNotExist()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "dummy-user-token");

        var response = await _client.PostAsJsonAsync("/graphql", new { query = "{ adminOnlyField }" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();

        var found = errors.EnumerateArray()
            .Select(e => e.GetProperty("message").GetString() ?? string.Empty)
            .Any(msg => msg.Contains("does not exist on type", StringComparison.OrdinalIgnoreCase));

        found.Should().BeTrue(
            $"Expected 'does not exist on type' error when calling admin field on user schema. Body: {body}");
    }
}
