using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.NUnit;
using WebApi.Data;

namespace Tests.GraphQL;

[TestFixture]
public class TestCommPaginationHttp
{
	private ConfigureApplicationFactory _factory = null!;
	private HttpClient _client = null!;

	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		_factory = new ConfigureApplicationFactory();
		_client = _factory.CreateClient();
		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dummy-user-token");

		// Seed in-memory test data
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();
		db.Communications.AddRange(
			new Communication { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Title = "Welcome Email", TypeCode = "GEN", CurrentStatusCode = "NEW", LastUpdatedUtc = DateTime.UtcNow },
			new Communication { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Title = "Follow Up", TypeCode = "GEN", CurrentStatusCode = "NEW", LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-1) },
			new Communication { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Title = "Survey Reminder", TypeCode = "GEN", CurrentStatusCode = "NEW", LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-2) },
			new Communication { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Title = "Final Notice", TypeCode = "GEN", CurrentStatusCode = "NEW", LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-3) }
		);

		db.SaveChanges();
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		_client?.Dispose();
		_factory?.Dispose();
	}

	private async Task<string> PostGraphQLAsync(string query)
	{
		var payload = new { query };
		var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

		using var resp = await _client.PostAsync("/graphql", content);
		var body = await resp.Content.ReadAsStringAsync();

		// TestContext.Out.WriteLine("HTTP Status: " + resp.StatusCode);
		// TestContext.Out.WriteLine("Response body:");
		// TestContext.Out.WriteLine(body);

		if (!resp.IsSuccessStatusCode)
		{
			Assert.Fail($"GraphQL HTTP call failed with status {(int)resp.StatusCode}: {resp.ReasonPhrase}\nBody: {body}");
		}

		return body;
	}

	[Test]
	public async Task Page1_First2Items_ReturnsTwoItemsAndHasNextPageTrue()
	{
		var query = @"
            query {
              communications(first: 2) {
                edges {
                  cursor
                  node { id title }
                }
                pageInfo {
                  hasNextPage
                  endCursor
                }
              }
            }";

		var body = await PostGraphQLAsync(query);

		Snapshot.Match(
			body,
			options => options
				.IgnoreField("$.data.communications.edges[*].cursor")
				.IgnoreField("$.extensions")
		);
	}

	[Test]
	public async Task Page2_UsesEndCursorFromPage1_ReturnsNextItems()
	{
		var firstQuery = @"
            query {
              communications(first: 2) {
                pageInfo { endCursor hasNextPage }
              }
            }";

		var firstBody = await PostGraphQLAsync(firstQuery);
		using var doc = JsonDocument.Parse(firstBody);

		var endCursor = doc.RootElement
			.GetProperty("data")
			.GetProperty("communications")
			.GetProperty("pageInfo")
			.GetProperty("endCursor")
			.GetString();

		Assert.That(endCursor, Is.Not.Null, "endCursor should not be null on first page");

		var secondQuery = $@"
            query {{
              communications(first: 2, after: ""{endCursor}"") {{
                edges {{
                  cursor
                  node {{ id title }}
                }}
                pageInfo {{
                  hasNextPage
                  endCursor
                }}
              }}
            }}";

		var secondBody = await PostGraphQLAsync(secondQuery);

		Snapshot.Match(
			secondBody,
			options => options
				.IgnoreField("$.data.communications.edges[*].cursor")
				.IgnoreField("$.extensions")
		);
	}

	[Test]
	public async Task CommunicationsResult_MatchesShape()
	{
		var query = @"
            query {
              communications(first: 1) {
                edges {
                  cursor
                  node { id title }
                }
                pageInfo {
                  hasNextPage
                  endCursor
                }
              }
            }";

		var body = await PostGraphQLAsync(query);

		Snapshot.Match(
			body,
			options => options
				.IgnoreField("$.data.communications.edges[*].cursor")
				.IgnoreField("$.extensions")
		);
	}
}

