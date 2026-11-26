using Microsoft.Extensions.DependencyInjection;
using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Infrastructure;
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

    public async Task InitializeAsync() // Start the SQL Server container and the test API host.
    {
        await _database.StartAsync();

        _factory = new ApiFactory(_database.GetConnectionString());
        Client = _factory.CreateClient();
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
