using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Oekolariet.Shared.Games;

namespace Oekolariet.Api.Tests;

// Run with Docker running. These tests start games through HTTP using the shared
// SQL Server container and verify how the API handles each exhibition state.
[Collection("ApiTests")]
public sealed class StartGameApiTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task StartGame_WithPublishedExhibition_ReturnsCreatedWithSortingGame()
    {
        // The development seed creates this exhibition with an active version.
        var request = new
        {
            exhibitionId = Guid.Parse("9b1d5e2a-0c4f-4e7a-9a51-3f1c0e8b0001"),
            avatarKey = "odder"
        };

        using var response = await fixture.Client.PostAsJsonAsync("/api/v1/games", request); // Start a game for the published exhibition.

        Assert.Equal(HttpStatusCode.Created, response.StatusCode); // The API should return 201 Created for a successful game start.

        var game = await response.Content.ReadFromJsonAsync<GameDto>();
        var sorting = Assert.IsType<SortingGameDto>(game); // "type": "Sorting" is the correct C# type now
        Assert.NotEqual(Guid.Empty, sorting.GameId);
        Assert.NotEmpty(sorting.Categories);
        Assert.NotEmpty(sorting.Items);
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

    // Test ensures no correct answer is send to the client
    [Fact]
    public async Task StartGame_NeverSendsCorrectAnswersToGuest()
    {
        var request = new
        {
            exhibitionId = Guid.Parse("9b1d5e2a-0c4f-4e7a-9a51-3f1c0e8b0001"),
            avatarKey = "odder"
        };

        using var response = await fixture.Client.PostAsJsonAsync("/api/v1/games", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Read the raw JSON and NOT a DTO
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = json.RootElement.GetProperty("items");

        Assert.NotEqual(0, items.GetArrayLength()); // Otherwise the test doesnt really prove anything
        foreach(var item in items.EnumerateArray())
        {
            Assert.False(item.TryGetProperty("CorrectCategoryId", out _), "ADR-0003: Guest must not recive correct answers");
            Assert.False(item.TryGetProperty("explanation", out _), "ADR-0003: Guest must not recive explanations");
        }

    }
}
