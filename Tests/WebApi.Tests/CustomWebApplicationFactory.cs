using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using WebApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace WebApi.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Add in-memory DB
            services.AddDbContext<CommunicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // Replace auth with stub
            services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

            // Set default scheme so GraphQL uses stub
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            });
        });
    }
}
