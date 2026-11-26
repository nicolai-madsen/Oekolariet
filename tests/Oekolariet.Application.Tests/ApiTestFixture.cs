using Microsoft.Extensions.DependencyInjection;
using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Domain.Content;
using Oekolariet.Infrastructure;
using Testcontainers.MsSql;


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
