using System.Net;
using System.Net.Http.Json;
using Oekolariet.Infrastructure.Seeding;
using Oekolariet.Shared.Games;

namespace Oekolariet.Api.Tests;

[Collection("ApiTests")]
public sealed class CompleteExpiredGameApiTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task CompleteExpiredGame_WithUnknownGame_ReturnsNotFound()
    {
        var unknownGameId = Guid.NewGuid();

        using var response = await fixture.Client.PostAsync(
            $"/api/v1/games/{unknownGameId}/complete-expired",
            content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CompleteExpiredGame_AtDeadline_AddsOneHighscoreEntryEvenAfterRetry()
    {
        var clock = new TestClock(
            new DateTimeOffset(2026, 11, 28, 10, 0, 0, TimeSpan.Zero));

        await using var factory = fixture.CreateFactoryWithClock(clock);
        using var client = factory.CreateClient();

        using var startResponse = await client.PostAsJsonAsync(
            "/api/v1/games",
            new StartGameRequest(SeedIds.Exhibition, "froe"));

        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);

        var result = await startResponse.Content.ReadFromJsonAsync<GameDto>();
        var game = Assert.IsType<SortingGameDto>(result);
        var completeUrl = $"/api/v1/games/{game.GameId}/complete-expired";
        var highscoresUrl = $"/api/v1/games/{game.GameId}/highscores";
        var deadline = new DateTimeOffset(game.EndsAtUtc, TimeSpan.Zero);

        clock.UtcNow = deadline.AddTicks(-1);
        using var earlyResponse = await client.PostAsync(completeUrl, content: null);
        Assert.Equal(HttpStatusCode.Conflict, earlyResponse.StatusCode);

        // The shared database may already contain scores from other rounds.
        var before = await client.GetFromJsonAsync<List<HighscoreEntryDto>>(highscoresUrl);
        Assert.NotNull(before);
        var previousGuestNumbers = before.Select(entry => entry.GuestNumber).ToHashSet();

        clock.UtcNow = deadline;
        using var completionResponse = await client.PostAsync(completeUrl, content: null);
        Assert.Equal(HttpStatusCode.NoContent, completionResponse.StatusCode);

        var after = await client.GetFromJsonAsync<List<HighscoreEntryDto>>(highscoresUrl);
        Assert.NotNull(after);
        Assert.Equal(before.Count + 1, after.Count);

        var added = Assert.Single(after, entry => !previousGuestNumbers.Contains(entry.GuestNumber));
        Assert.Equal("froe", added.AvatarKey);
        Assert.Equal(0, added.Score);

        clock.UtcNow = deadline.AddSeconds(5);
        using var retryResponse = await client.PostAsync(completeUrl, content: null);
        Assert.Equal(HttpStatusCode.NoContent, retryResponse.StatusCode);

        var afterRetry = await client.GetFromJsonAsync<List<HighscoreEntryDto>>(highscoresUrl);
        Assert.NotNull(afterRetry);
        Assert.Equal(after.Count, afterRetry.Count);
        Assert.Single(afterRetry, entry => entry.GuestNumber == added.GuestNumber);
    }

   
}
