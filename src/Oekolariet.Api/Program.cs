using Oekolariet.Api.Endpoints;
using Oekolariet.Application.Drafts;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Oekolariet")
    ?? throw new InvalidOperationException("Connection string 'Oekolariet' mangler i appsettings.");

builder.Services.AddInfrastructure(connectionString, seedDevelopmentData: builder.Environment.IsDevelopment());
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<GetActiveVersionHandler>();
builder.Services.AddScoped<CreateDraftHandler>();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

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

app.MapGuestExhibitionEndpoints();

await app.RunAsync();
