using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Shared.Interfaces;
using WebApi.Data;
using WebApi.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment()) { builder.Configuration.AddUserSecrets<Program>(); }
else if (builder.Environment.IsProduction()) { builder.Configuration.AddUserSecrets<Program>(); }

// Database
builder.Services.AddDbContext<CommunicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqldata")));

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbit")
        ?? throw new InvalidOperationException("Missing RabbitMQ connection string");
    return new ConnectionFactory { Uri = new Uri(rabbitConnectionString) };
});

// Problem details + health checks
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// Dependency Injection
builder.Services
    .AddScoped<ICommService, CommService>()
    .AddScoped<ITypeService, TypeService>()
    .AddScoped<IGlobalStatusService, GlobalStatusService>();

builder.Services.AddHostedService<RabbitMqSubscriberService>();
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
var audience = builder.Configuration["Okta:Audience"] ?? "api://default";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;

        // FIX: Allow HTTP metadata in development (but Authority stays HTTPS)
        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }
        else
        {
            options.RequireHttpsMetadata = true;
        }

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

// Authorization
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy =>
    {
        policy.RequireRole("Admin");
    })
    .AddPolicy("User", policy =>
    {
        policy.RequireRole("User");
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

// FIX: Only redirect to HTTPS for external requests in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();