using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using BlazorServer.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment()) { builder.Configuration.AddUserSecrets<Program>(); }
else if (builder.Environment.IsProduction()) { builder.Configuration.AddUserSecrets<Program>(); }

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbit") 
        ?? throw new InvalidOperationException("Missing RabbitMQ connection string");
    return new ConnectionFactory { Uri = new Uri(rabbitConnectionString) };
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();

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

    // For development to allow HTTP metadata
    if (builder.Environment.IsDevelopment())
    {
        options.RequireHttpsMetadata = false;
    }

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

// Attach access token to API requests
builder.Services.AddTransient<AccessTokenHandler>();

var apiBase = builder.Configuration["WebApiEndpointHttps"];
if (string.IsNullOrWhiteSpace(apiBase))
{
    throw new InvalidOperationException("Missing WebApiEndpointHttps");
}

builder.Services.AddHttpClient<CommService>(client => client.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddHttpClient<TypeService>(client => client.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddHttpClient<GlobalStatusService>(client => client.BaseAddress = new Uri(apiBase))
    .AddHttpMessageHandler<AccessTokenHandler>();

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

app.MapHealthChecks("/health");
app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
