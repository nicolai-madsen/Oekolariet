using Oekolariet.Api.Endpoints;
using Oekolariet.Api.ErrorHandling;
using Oekolariet.Application.Drafts;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Infrastructure;
using Oekolariet.Application.Games;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Oekolariet.Api.Auth;
using Oekolariet.Application.Auth;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Oekolariet")
    ?? throw new InvalidOperationException("Connection string 'Oekolariet' mangler i appsettings.");

builder.Services.AddInfrastructure(connectionString, seedDevelopmentData: builder.Environment.IsDevelopment());
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<GetActiveVersionHandler>();
builder.Services.AddScoped<CreateDraftHandler>();
builder.Services.AddScoped<StartGameHandler>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// JWT login: read Issuer and Audience from appsettings.json, and the secret Key from user-secrets
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing. Set it with dotnet user-secrets.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Rules for checking a token that a client sends
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.Services.MigrateDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapDraftEndpoints();
app.MapGuestExhibitionEndpoints();
app.MapGameEndpoints();

await app.RunAsync();

public partial class Program { } // For integration testing
