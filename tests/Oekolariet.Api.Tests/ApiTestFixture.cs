using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Domain.Content;
using Oekolariet.Infrastructure;
using Oekolariet.Shared.Auth;
using Testcontainers.MsSql;
using Microsoft.EntityFrameworkCore;


namespace Oekolariet.Api.Tests;

// All test classes in the ApiTests collection share one SQL Server container and API host.
public sealed class ApiTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _database =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build(); // Use SQL Server to ensure that two games starting at the same time receive different guest numbers.

    private ApiFactory? _factory; // The test API host, which is disposed when the fixture is disposed.

    public HttpClient Client { get; private set; } = null!; // The HTTP client for sending requests to the test API.

    // A client that is logged in as the test editor (from the Development seed), for CMS endpoints
    public HttpClient EditorClient { get; private set; } = null!;

    public async Task InitializeAsync() // Start the SQL Server container and the test API host.
    {
        await _database.StartAsync();

        _factory = new ApiFactory(_database.GetConnectionString());
        Client = _factory.CreateClient();

        EditorClient = _factory.CreateClient();
        var token = await LoginAsTestEditorAsync();
        EditorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // Logs in as the test editor and returns the token.
    // This email and password are the development test editor from DevelopmentSeeder (not a real account).
    private async Task<string> LoginAsTestEditorAsync()
    {
        var request = new LoginRequestDto("admin@oekolariet.test", "Udvikling123!");
        using var response = await Client.PostAsJsonAsync("/api/v1/auth/login", request);
        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return login!.AccessToken;
    }

    public async Task<Guid> CreateUnpublishedExhibitionAsync() // Create an unpublished exhibition in the test database and return its ID.
    {
        var factory = _factory ?? throw new InvalidOperationException("The test API has not started.");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OekolarietDbContext>();

        var exhibition = Exhibition.Create(Guid.NewGuid(), "Testudstilling", DateTime.UtcNow);
        db.Set<Exhibition>().Add(exhibition);
        await db.SaveChangesAsync();

        return exhibition.Id;
    }

    public async Task<string> GetSavedAvatarKeyAsync(Guid gameId)
    {
        var factory = _factory ?? throw new InvalidOperationException("The test API has not started.");

        // Read through a fresh DbContext to verify what the API actually saved.
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OekolarietDbContext>();

        return await db.Games
            .Where(game => game.Id == gameId)
            .Select(game => game.AvatarKey)
            .SingleAsync();
    }
    public async Task<Guid> PublishVersionAsync(Guid exhibitionId, ExhibitionContent content)
    {
        var factory = _factory ?? throw new InvalidOperationException("The test API has not started.");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OekolarietDbContext>();

        var exhibition = await db.Set<Exhibition>().FindAsync(exhibitionId)
            ?? throw new InvalidOperationException($"Test exhibition {exhibitionId} was not found.");

        // Publish updates the exhibition's active version and returns the version to save.
        var version = exhibition.Publish(content, "test@oekolariet.local", DateTime.UtcNow);
        db.ExhibitionVersions.Add(version);
        await db.SaveChangesAsync();

        return version.Id;
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (_factory is not null)
            {
                await _factory.DisposeAsync();
            }
        }
        finally
        {
            await _database.DisposeAsync();
        }
    }
}
