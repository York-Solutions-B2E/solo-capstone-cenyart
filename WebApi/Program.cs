using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Shared.Interfaces;
using WebApi.Data;
using WebApi.Services;
using System.Security.Claims;
using HotChocolate.AspNetCore;
using WebApi.GraphQL;

var builder = WebApplication.CreateBuilder(args);

// ---------------- User Secrets ----------------
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// ---------------- Database ----------------
// Register SQL Server only if not testing
if (!builder.Environment.IsEnvironment("Testing"))
{
    var sqlConnectionString = builder.Configuration.GetConnectionString("sqldata")
        ?? throw new InvalidOperationException("Missing SQL Server connection string: 'sqldata'");

    builder.Services.AddDbContext<CommunicationDbContext>(options =>
        options.UseSqlServer(sqlConnectionString));
}

// ---------------- RabbitMQ ----------------
// Register RabbitMQ only if not testing
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSingleton<IConnectionFactory>(sp =>
    {
        var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbit")
            ?? throw new InvalidOperationException("Missing RabbitMQ connection string");
        return new ConnectionFactory { Uri = new Uri(rabbitConnectionString) };
    });

    builder.Services.AddHostedService<RabbitMqSubscriberService>();
}

// ---------------- Infra ----------------
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// ---------------- DI ----------------
builder.Services
    .AddScoped<ICommService, CommService>()
    .AddScoped<ITypeService, TypeService>()
    .AddScoped<IGlobalStatusService, GlobalStatusService>();

builder.Services.AddControllers();

// ---------------- Swagger + Auth ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Communication API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var authority = $"{builder.Configuration["Okta:OktaDomain"]}/oauth2/default";
var audience = builder.Configuration["Okta:Audience"] ?? "api://default";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = !builder.Environment.IsEnvironment("Testing");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"))
    .AddPolicy("User", policy => policy.RequireRole("User"));

// ---------------- GraphQL ----------------
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .DisableIntrospection(!builder.Environment.IsDevelopment() &&
                          !builder.Environment.IsEnvironment("Testing"));

// ---------------- Build app ----------------
var app = builder.Build();

// ---------------- DB Migrations ----------------
if (!builder.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();
    db.Database.Migrate();
    await DatabaseSeeder.SeedAsync(db);
}

// ---------------- Middleware ----------------
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!builder.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL("/graphql")
   .RequireAuthorization()
   .WithOptions(new GraphQLServerOptions
   {
        Tool = { Enable = app.Environment.IsDevelopment() }
   });

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

// Make Program accessible to WebApplicationFactory
public partial class Program { }