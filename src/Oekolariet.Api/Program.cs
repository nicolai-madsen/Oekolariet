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
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Oekolariet")
    ?? throw new InvalidOperationException("Connection string 'Oekolariet' mangler i appsettings.");

builder.Services.AddInfrastructure(connectionString, seedDevelopmentData: builder.Environment.IsDevelopment());
builder.Services.AddSingleton(TimeProvider.System);

// ExhibitionVersion services
builder.Services.AddScoped<GetActiveVersionHandler>();

// Draft services
builder.Services.AddScoped<CreateDraftHandler>();
builder.Services.AddScoped<GetDraftHandler>();

// Game services
builder.Services.AddScoped<StartGameHandler>();
builder.Services.AddScoped<SubmitAnswerHandler>();
builder.Services.AddScoped<GetHighscoresHandler>();
builder.Services.AddScoped<CompleteExpiredGameHandler>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// JWT login: read Issuer and Audience from appsettings.json, and the secret Key from user-secrets
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key is missing or too short (min. 32 characters). Set it with dotnet user-secrets, or JWT_KEY in .env for Docker.");
}

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

builder.Services.AddAuthorization(options =>
{
    // Default deny: every endpoint needs a logged-in Editor,
    // unless it says AllowAnonymous (guest endpoints and login)
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireRole(Roles.Editor)
        .Build();
});
builder.Services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
builder.Services.AddScoped<LoginHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous(); // the API description (openapi.json) can be read without login
    await app.Services.MigrateDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapDraftEndpoints();
app.MapGuestExhibitionEndpoints();
app.MapGameEndpoints();
app.MapAuthEndpoints();

await app.RunAsync();

public partial class Program { } // For integration testing
