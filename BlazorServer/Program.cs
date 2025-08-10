using BlazorServer.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// 1️⃣ Aspire defaults (loads WebApiEndpoint, RabbitMQ, etc.)
builder.AddServiceDefaults();

// 2️⃣ Register Razor Pages & Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();   // required for blazor.server.js


var apiBase = builder.Configuration["WebApiEndpointHttps"];
if (string.IsNullOrWhiteSpace(apiBase))
{
    throw new InvalidOperationException("Missing WebApiEndpointHttps");
}
builder.Services.AddHttpClient<CommService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});
builder.Services.AddHttpClient<TypeService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});
builder.Services.AddHttpClient<GlobalStatusService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});

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
