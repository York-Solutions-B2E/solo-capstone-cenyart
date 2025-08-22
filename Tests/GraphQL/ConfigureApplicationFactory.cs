using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using WebApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace Tests.GraphQL;

public class ConfigureApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Add in-memory DB
            services.AddDbContext<CommunicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // Register test authentication scheme AND set it as the default auth scheme.
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, AuthHandler>("TestScheme", options => { });
        });
    }
}
