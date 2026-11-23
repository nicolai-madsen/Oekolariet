using System.Net;
using System.Net.Http.Json;

namespace Oekolariet.Api.Tests;

// Run with Docker running. These tests start games through HTTP using the shared
// SQL Server container and verify how the API handles each exhibition state.
[Collection("ApiTests")]
public sealed class StartGameApiTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task StartGame_WithPublishedExhibition_ReturnsCreatedAndGameId()
    {
        // The development seed creates this exhibition with an active version.
        var request = new
        {
            exhibitionId = Guid.Parse("9b1d5e2a-0c4f-4e7a-9a51-3f1c0e8b0001"),
            avatarKey = "odder"
        };

        using var response = await fixture.Client.PostAsJsonAsync("/api/v1/games", request); // Start a game for the published exhibition.

        Assert.Equal(HttpStatusCode.Created, response.StatusCode); // The API should return 201 Created for a successful game start.

        var result = await response.Content.ReadFromJsonAsync<StartGameResponse>(); // Deserialize the response to get the game ID.
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.GameId);
    }

    [Fact]
    public async Task StartGame_WithUnknownExhibition_ReturnsNotFound() // Attempt to start a game for an exhibition that does not exist.
    {
        var request = new
        {
            exhibitionId = Guid.NewGuid(),
            avatarKey = "odder"
        };

        using var response = await fixture.Client.PostAsJsonAsync("/api/v1/games", request); // Start a game for the unknown exhibition.

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); // The API should return 404 Not Found for an unknown exhibition.
    }

    [Fact]
    public async Task StartGame_WithUnpublishedExhibition_ReturnsConflict() // Attempt to start a game for an exhibition that exists but is not published.
    {
        var exhibitionId = await fixture.CreateUnpublishedExhibitionAsync();
        var request = new
        {
            exhibitionId,
            avatarKey = "odder"
        };

        using var response = await fixture.Client.PostAsJsonAsync("/api/v1/games", request); // Start a game for the unpublished exhibition.

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode); // The API should return 409 Conflict for an unpublished exhibition, indicating that the game cannot be started.
    }

    private sealed record StartGameResponse(Guid GameId); // A simple record to deserialize the response from the Start Game API, containing the ID of the newly started game.
}
