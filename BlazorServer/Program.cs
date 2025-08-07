using BlazorServer.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Aspire defaults (loads WebApiEndpoint, RabbitMQ, etc.)
builder.AddServiceDefaults();

// 2️⃣ Register Razor Pages & Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();   // required for blazor.server.js

// 3️⃣ MudBlazor
builder.Services.AddMudServices();

// 4️⃣ HTTP Client / ApiServices (typed clients)
var apiBase = builder.Configuration["WebApiEndpointHttps"];
if (string.IsNullOrWhiteSpace(apiBase))
{
    throw new InvalidOperationException("API URL is missing. Please set 'WebApiEndpointHttps' in appsettings.json or environment variables.");
}
builder.Services.AddHttpClient<CommService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});
builder.Services.AddHttpClient<TypeService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});
builder.Services.AddHttpClient<StatusService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});


// ... other services ...

var app = builder.Build();

// 5️⃣ Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 6️⃣ Endpoint mapping
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
