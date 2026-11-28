using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oekolariet.Application.Exhibitions;
using Oekolariet.Infrastructure;
using Oekolariet.Infrastructure.Seeding;
using Oekolariet.Shared.Games;

namespace Oekolariet.Api.Tests;

[Collection("ApiTests")]
public sealed class HighscoreApiTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task GetHighscores_UsesDanishCalendarDaysAndMonths()
    {
        // Denmark is UTC+2 in July. All test times below are expressed in UTC.
        var clock = new TestClock(
            new DateTimeOffset(2026, 6, 30, 21, 55, 0, TimeSpan.Zero));

        await using var factory = fixture.CreateFactoryWithClock(clock);
        using var client = factory.CreateClient();

        // June 30 at 23:55 in Denmark.
        var juneGame = await StartAndCompleteRoundAsync(client, clock);

        // July 1 at 00:05 in Denmark, while the UTC date is still June 30.
        clock.UtcNow = new DateTimeOffset(2026, 6, 30, 22, 5, 0, TimeSpan.Zero);
        var firstJulyGame = await StartAndCompleteRoundAsync(client, clock);

        // July 2 at 00:05 in Denmark, while the UTC date is still July 1.
        clock.UtcNow = new DateTimeOffset(2026, 7, 1, 22, 5, 0, TimeSpan.Zero);
        var todayGame = await StartAndCompleteRoundAsync(client, clock);

        clock.UtcNow = new DateTimeOffset(2026, 7, 2, 10, 0, 0, TimeSpan.Zero);
        var url = $"/api/v1/games/{todayGame.GameId}/highscores";

        var daily = await client.GetFromJsonAsync<List<HighscoreEntryDto>>(url);

        Assert.NotNull(daily);
        Assert.DoesNotContain(daily, row => row.GuestNumber == juneGame.GuestNumber);
        Assert.DoesNotContain(daily, row => row.GuestNumber == firstJulyGame.GuestNumber);
        Assert.Contains(daily, row => row.GuestNumber == todayGame.GuestNumber);

        var monthly = await client.GetFromJsonAsync<List<HighscoreEntryDto>>(
            $"{url}?period=month");

        Assert.NotNull(monthly);
        Assert.DoesNotContain(monthly, row => row.GuestNumber == juneGame.GuestNumber);
        Assert.Contains(monthly, row => row.GuestNumber == firstJulyGame.GuestNumber);
        Assert.Contains(monthly, row => row.GuestNumber == todayGame.GuestNumber);
    }

    [Fact]
    public async Task GetHighscores_WithEqualScores_ReturnsMostRecentlyCompletedRoundFirst()
    {
        var clock = new TestClock(
            new DateTimeOffset(2026, 7, 2, 10, 0, 0, TimeSpan.Zero));

        await using var factory = fixture.CreateFactoryWithClock(clock);
        using var client = factory.CreateClient();

        // Both guests finish with zero points. The second round starts later.
        var firstGame = await StartAndCompleteRoundAsync(client, clock);
        clock.UtcNow = clock.UtcNow.AddMinutes(1);
        var secondGame = await StartAndCompleteRoundAsync(client, clock);

        var highscores = await client.GetFromJsonAsync<List<HighscoreEntryDto>>(
            $"/api/v1/games/{secondGame.GameId}/highscores");

        Assert.NotNull(highscores);

        // Keep the API's ordering while ignoring guests from other tests.
        var entries = highscores.Where(entry =>
            entry.GuestNumber == firstGame.GuestNumber ||
            entry.GuestNumber == secondGame.GuestNumber);

        Assert.Collection(
            entries,
            newest =>
            {
                Assert.Equal(secondGame.GuestNumber, newest.GuestNumber);
                Assert.Equal(0, newest.Score);
            },
            oldest =>
            {
                Assert.Equal(firstGame.GuestNumber, oldest.GuestNumber);
                Assert.Equal(0, oldest.Score);
            });
    }

    [Fact]
    public async Task GetHighscores_WithDifferentScores_ReturnsHigherScoreFirst()
    {
        var clock = new TestClock(
            new DateTimeOffset(2026, 7, 3, 10, 0, 0, TimeSpan.Zero));

        await using var factory = fixture.CreateFactoryWithClock(clock);
        using var client = factory.CreateClient();

        using var startResponse = await client.PostAsJsonAsync(
            "/api/v1/games", new StartGameRequest(SeedIds.Exhibition, "pindsvin"));

        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);

        var result = await startResponse.Content.ReadFromJsonAsync<GameDto>();
        var higherScoringGame = Assert.IsType<SortingGameDto>(result);

        // Read the correct category from the published version used by this round.
        var item = higherScoringGame.Items.First();
        await using var scope = factory.Services.CreateAsyncScope();
        var versions = scope.ServiceProvider.GetRequiredService<IExhibitionVersionRepository>();
        var version = await versions.GetByIdAsync(higherScoringGame.VersionId, CancellationToken.None);
        Assert.NotNull(version);

        var sortingItem = version.Content.Game.Items.Single(entry => entry.Id == item.Id);

        using var answerResponse = await client.PostAsJsonAsync(
            $"/api/v1/games/{higherScoringGame.GameId}/answers",
            new AnswerRequest(item.Id, sortingItem.CorrectCategoryId));

        Assert.Equal(HttpStatusCode.OK, answerResponse.StatusCode);

        clock.UtcNow = new DateTimeOffset(higherScoringGame.EndsAtUtc, TimeSpan.Zero);
        using var completeResponse = await client.PostAsync(
            $"/api/v1/games/{higherScoringGame.GameId}/complete-expired", content: null);

        Assert.Equal(HttpStatusCode.NoContent, completeResponse.StatusCode);

        // The older ten-point round must rank above the newer zero-point round.
        clock.UtcNow = clock.UtcNow.AddMinutes(1);
        var newerGame = await StartAndCompleteRoundAsync(client, clock);

        var highscores = await client.GetFromJsonAsync<List<HighscoreEntryDto>>(
            $"/api/v1/games/{newerGame.GameId}/highscores");

        Assert.NotNull(highscores);

        var entries = highscores.Where(entry =>
            entry.GuestNumber == higherScoringGame.GuestNumber ||
            entry.GuestNumber == newerGame.GuestNumber);

        Assert.Collection(
            entries,
            first =>
            {
                Assert.Equal(higherScoringGame.GuestNumber, first.GuestNumber);
                Assert.Equal(10, first.Score);
            },
            second =>
            {
                Assert.Equal(newerGame.GuestNumber, second.GuestNumber);
                Assert.Equal(0, second.Score);
            });
    }

    [Fact]
    public async Task GetHighscores_AfterPublishingNewVersion_StartsWithEmptyListAndKeepsOldScore()
    {
        var clock = new TestClock(
            new DateTimeOffset(2026, 7, 4, 10, 0, 0, TimeSpan.Zero));

        await using var factory = fixture.CreateFactoryWithClock(clock);
        using var client = factory.CreateClient();

        var exhibitionId = await fixture.CreateUnpublishedExhibitionAsync();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OekolarietDbContext>();
        var exhibition = await db.Exhibitions.SingleAsync(e => e.Id == exhibitionId);

        // Publish the same quiz content twice to isolate the effect of the version change.
        var content = await db.ExhibitionVersions
            .Where(v => v.ExhibitionId == SeedIds.Exhibition)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => v.Content)
            .FirstAsync();

        var firstVersion = exhibition.Publish(
            content, "editor@oekolariet.test", clock.GetUtcNow().UtcDateTime);

        db.ExhibitionVersions.Add(firstVersion);
        await db.SaveChangesAsync();

        var firstGame = await StartAndCompleteRoundAsync(client, clock, exhibitionId);
        Assert.Equal(firstVersion.Id, firstGame.VersionId);

        var secondVersion = exhibition.Publish(
            content, "editor@oekolariet.test", clock.GetUtcNow().UtcDateTime);

        db.ExhibitionVersions.Add(secondVersion);
        await db.SaveChangesAsync();

        using var startResponse = await client.PostAsJsonAsync(
            "/api/v1/games", new StartGameRequest(exhibitionId, "pindsvin"));

        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);

        var result = await startResponse.Content.ReadFromJsonAsync<GameDto>();
        var secondGame = Assert.IsType<SortingGameDto>(result);
        Assert.Equal(secondVersion.Id, secondGame.VersionId);
        Assert.NotEqual(firstGame.VersionId, secondGame.VersionId);

        var newScores = await client.GetFromJsonAsync<List<HighscoreEntryDto>>(
            $"/api/v1/games/{secondGame.GameId}/highscores");

        Assert.NotNull(newScores);
        Assert.Empty(newScores);

        var oldScores = await client.GetFromJsonAsync<List<HighscoreEntryDto>>(
            $"/api/v1/games/{firstGame.GameId}/highscores");

        Assert.NotNull(oldScores);
        var oldEntry = Assert.Single(oldScores);
        Assert.Equal(firstGame.GuestNumber, oldEntry.GuestNumber);
        Assert.Equal(0, oldEntry.Score);
    }

    private static async Task<SortingGameDto> StartAndCompleteRoundAsync(
        HttpClient client,
        TestClock clock,
        Guid? exhibitionId = null)
    {
        using var startResponse = await client.PostAsJsonAsync(
            "/api/v1/games",
            new StartGameRequest(exhibitionId ?? SeedIds.Exhibition, "froe"));

        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);

        var result = await startResponse.Content.ReadFromJsonAsync<GameDto>();
        var game = Assert.IsType<SortingGameDto>(result);

        clock.UtcNow = new DateTimeOffset(game.EndsAtUtc, TimeSpan.Zero);

        using var completeResponse = await client.PostAsync(
            $"/api/v1/games/{game.GameId}/complete-expired", content: null);

        Assert.Equal(HttpStatusCode.NoContent, completeResponse.StatusCode);
        return game;
    }
}
