using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Shared.Interfaces;
using WebApi.Data;
using WebApi.Services;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Aspire connections
var sqlConnectionString = builder.Configuration.GetConnectionString("sqldata") 
    ?? throw new InvalidOperationException("Missing SQL Server connection string");
builder.Services.AddDbContext<CommunicationDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbit") 
        ?? throw new InvalidOperationException("Missing RabbitMQ connection string");
    return new ConnectionFactory { Uri = new Uri(rabbitConnectionString) };
});

builder.AddServiceDefaults();

// Problem details + health checks
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// Load user secrets in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Dependency Injection
builder.Services
    .AddScoped<ICommService, CommService>()
    .AddScoped<ITypeService, TypeService>()
    .AddScoped<IGlobalStatusService, GlobalStatusService>();

builder.Services.AddHostedService<RabbitMqSubService>();
builder.Services.AddControllers();

// Swagger + JWT Auth
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
var audience = "api://default";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                Console.WriteLine($"[OnMessageReceived] Raw token: {ctx.Token}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("[OnAuthenticationFailed] Exception:");
                Console.WriteLine(ctx.Exception?.ToString() ?? "No exception");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                Console.WriteLine("[OnTokenValidated] SUCCESS for user: " +
                                  (ctx.Principal?.Identity?.Name ?? "(no name)"));
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                Console.WriteLine("[OnChallenge] Triggered");
                Console.WriteLine($"  Error: {ctx.Error ?? "(null)"}");
                Console.WriteLine($"  Description: {ctx.ErrorDescription ?? "(null)"}");
                Console.WriteLine($"  Uri: {ctx.ErrorUri ?? "(null)"}");
                Console.WriteLine($"  Headers sent? {ctx.Response.HasStarted}");
                return Task.CompletedTask;
            },
            OnForbidden = ctx =>
            {
                Console.WriteLine("[OnForbidden] Triggered");
                return Task.CompletedTask;
            }
        };
    });



// Authorization
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
    });

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();
    db.Database.Migrate();
    await DatabaseSeeder.SeedAsync(db);
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
