using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Infrastructure;
using Testcontainers.MsSql;


namespace Oekolariet.Api.Tests;

// Run these API integration tests with Docker running. Each test starts a fresh SQL Server
// container and sends an HTTP request to the test API. Together they verify that starting
// a game returns 201 for a published exhibition, 404 for an unknown one, and 409 for an unpublished one.
public sealed class StartGameApiTests
{
    [Fact]
    public async Task StartGame_WithPublishedExhibition_ReturnsCreatedAndGameId()
    {
        await using var database = new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-latest").Build(); // Use SQL Server to ensure that two games starting at the same time receive different guest numbers.

        await database.StartAsync();

        await using var factory = new GameApiFactory(database.GetConnectionString());
        using var client = factory.CreateClient();

        // Use the published exhibition from the development seed data.
        var request = new
        {
            exhibitionId = "9b1d5e2a-0c4f-4e7a-9a51-3f1c0e8b0001",
            avatarKey = "odder"
        };

        using var response = await client.PostAsJsonAsync("/api/v1/games", request); // Start a game for the published exhibition.

        Assert.Equal(HttpStatusCode.Created, response.StatusCode); // 201 Created

        var result = await response.Content.ReadFromJsonAsync<StartGameResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.GameId);
    }

    private sealed record StartGameResponse(Guid GameId);

    [Fact]
    public async Task StartGame_WithUnknownExhibition_ReturnsNotFound()
    {
        await using var database = new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-latest").Build();

        await database.StartAsync();

        await using var factory = new GameApiFactory(database.GetConnectionString());
        using var client = factory.CreateClient();

        // Use a random exhibition ID that does not exist in the database.
        var request = new
        {
            exhibitionId = Guid.NewGuid(),
            avatarKey = "odder"
        };

        using var response = await client.PostAsJsonAsync("/api/v1/games", request); // Attempt to start a game for an unknown exhibition.

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); // 404 Not Found
    }

    [Fact]
    public async Task StartGame_WithUnpublishedExhibition_ReturnsConflict()
    {
        await using var database = new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-latest").Build(); // Use SQL Server to ensure that two games starting at the same time receive different guest numbers.

        await database.StartAsync();

        await using var factory = new GameApiFactory(database.GetConnectionString());
        using var client = factory.CreateClient();

        var exhibitionId = Guid.NewGuid();

        // Save the exhibition without publishing it, so it has no active version.
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OekolarietDbContext>();
            var exhibition = Exhibition.Create(exhibitionId, "Upubliseret udstilling", DateTime.UtcNow);

            db.Set<Exhibition>().Add(exhibition);
            await db.SaveChangesAsync();
        }
        // Attempt to start a game for the unpublished exhibition.
        var request = new
        {
            exhibitionId,
            avatarKey = "odder"
        };

        using var response = await client.PostAsJsonAsync("/api/v1/games", request); // Attempt to start a game for an unpublished exhibition.

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode); // 409 Conflict
    }
}
