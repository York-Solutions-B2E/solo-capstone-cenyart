using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using BlazorServer.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Load user secrets in Development only
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Aspire defaults to inject env variables, connection strings, etc.
builder.AddServiceDefaults();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();

// Authentication (Okta)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = $"{builder.Configuration["Okta:OktaDomain"]}/oauth2/default";
    options.ClientId = builder.Configuration["Okta:ClientId"];
    options.ClientSecret = builder.Configuration["Okta:ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("offline_access");
    options.Scope.Add("api://default");

    options.GetClaimsFromUserInfoEndpoint = true;
    options.TokenValidationParameters.NameClaimType = "name";
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy =>
    {
        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

// HttpClients for WebApi services, use Aspire-injected URL only (no fallback)
var apiBase = builder.Configuration["services:webapi:http:0"]
    ?? throw new InvalidOperationException("Missing Aspire injected WebAPI HTTP endpoint URL.");

builder.Services.AddHttpClient<CommService>(client => client.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddHttpClient<TypeService>(client => client.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddHttpClient<GlobalStatusService>(client => client.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddTransient<AccessTokenHandler>();

// Register RabbitMQ connection factory from Aspire "rabbit" connection string only
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbit")
        ?? throw new InvalidOperationException("Missing RabbitMQ connection string");

    return new ConnectionFactory { Uri = new Uri(rabbitConnectionString) };
});

builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
