using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Interfaces;
using WebApi.Services;
using WebApi.Data;
using WebApi.Data.Repositories.Interfaces;
using WebApi.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CommunicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqldata")));

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// Load user secrets in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Okta Token Service (for testing)
builder.Services.AddHttpClient<ITokenTestService, TokenTestService>();
builder.Services.AddScoped<ITokenTestService, TokenTestService>();

// Repository pattern
builder.Services.AddScoped<ICommunicationTypeRepository, CommunicationTypeRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Business services
builder.Services.AddScoped<ICommunicationTypeService, CommunicationTypeService>();
builder.Services.AddScoped<IStatusTaxonomyService, StatusTaxonomyService>();

builder.Services.AddControllers();

// Swagger + JWT Auth
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MvpOkta API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication (Okta)
var authority = builder.Configuration["Okta:OktaDomain"] + "/oauth2/default";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = "api://default";
        options.RequireHttpsMetadata = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ApiScope", policy =>
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
