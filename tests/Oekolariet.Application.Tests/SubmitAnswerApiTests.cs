using System.Net;
using System.Net.Http.Json;
using Oekolariet.Domain.Content;
using Oekolariet.Infrastructure.Seeding;
using Oekolariet.Shared.Games;

namespace Oekolariet.Api.Tests;

// Uses the shared SQL Server container to verify that an item can only be answered once per game.
[Collection("ApiTests")]
public sealed class SubmitAnswerApiTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task SubmitAnswer_WhenSameItemIsSubmittedTwice_ReturnsOkThenConflict()
    {
        using var startResponse = await fixture.Client.PostAsJsonAsync(
            "/api/v1/games", new StartGameRequest(SeedIds.Exhibition, "odder"));

        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);

        var game = Assert.IsType<SortingGameDto>(
            await startResponse.Content.ReadFromJsonAsync<GameDto>());

        // The seed assigns new IDs, so use the IDs returned for this round.
        var bottle = game.Items.Single(item => item.Name == "Flaske");
        var plastic = game.Categories.Single(category => category.Name == "Plast");
        var answer = new AnswerRequest(bottle.Id, plastic.Id);
        var path = $"/api/v1/games/{game.GameId}/answers";

        using var firstResponse = await fixture.Client.PostAsJsonAsync(path, answer);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var result = await firstResponse.Content.ReadFromJsonAsync<AnswerResultDto>();
        Assert.NotNull(result);
        Assert.True(result.IsCorrect);
        Assert.Equal(plastic.Id, result.CorrectCategoryId);
        Assert.Equal(1, result.AnsweredCount);

        using var secondResponse = await fixture.Client.PostAsJsonAsync(path, answer);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task SubmitAnswer_WhenNewVersionIsPublished_UsesVersionFromGameStart()
    {
        var exhibitionId = await fixture.CreateUnpublishedExhibitionAsync();
        var plastic = new Category(Guid.NewGuid(), "Plast", null);
        var cardboard = new Category(Guid.NewGuid(), "Pap", null);
        var item = new SortingItem(Guid.NewGuid(), "Testgenstand", null, plastic.Id, null);

        var quiz = new SortingGame("Testquiz", "Vælg en spand", 300, [plastic, cardboard], [item]);
        var content = ExhibitionContent.Empty with { Game = quiz };
        var firstVersionId = await fixture.PublishVersionAsync(exhibitionId, content);

        using var startResponse = await fixture.Client.PostAsJsonAsync(
            "/api/v1/games", new StartGameRequest(exhibitionId, "odder"));

        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);
        var game = Assert.IsType<SortingGameDto>(
            await startResponse.Content.ReadFromJsonAsync<GameDto>());
        Assert.Equal(firstVersionId, game.VersionId);

        // Keep the item and category IDs so only the correct answer changes between versions.
        var updatedItem = item with { CorrectCategoryId = cardboard.Id };
        var updatedContent = content with { Game = quiz with { Items = [updatedItem] } };
        var secondVersionId = await fixture.PublishVersionAsync(exhibitionId, updatedContent);
        Assert.NotEqual(firstVersionId, secondVersionId);

        using var answerResponse = await fixture.Client.PostAsJsonAsync(
            $"/api/v1/games/{game.GameId}/answers", new AnswerRequest(item.Id, plastic.Id));

        Assert.Equal(HttpStatusCode.OK, answerResponse.StatusCode);
        var result = await answerResponse.Content.ReadFromJsonAsync<AnswerResultDto>();
        Assert.NotNull(result);
        Assert.True(result.IsCorrect);
        Assert.Equal(plastic.Id, result.CorrectCategoryId);
    }

    [Fact]
    public async Task SubmitAnswer_OnlyFinalAnswerAddsRoundToHighscoresWithFinalScore()
    {
        var request = new StartGameRequest(SeedIds.Exhibition, "pindsvin");

        using var startResponse = await fixture.Client.PostAsJsonAsync(
            "/api/v1/games", request);

        Assert.Equal(HttpStatusCode.Created, startResponse.StatusCode);

        var result = await startResponse.Content.ReadFromJsonAsync<GameDto>();
        var game = Assert.IsType<SortingGameDto>(result);

        // A round needs at least two items to test the state between answers.
        Assert.True(game.Items.Count >= 2);

        var answersUrl = $"/api/v1/games/{game.GameId}/answers";
        var highscoresUrl = $"/api/v1/games/{game.GameId}/highscores";
        var categoryId = game.Categories.First().Id;
        var expectedScore = 0;

        for (var index = 0; index < game.Items.Count; index++)
        {
            var item = game.Items[index];
            var isFinalAnswer = index == game.Items.Count - 1;

            using var answerResponse = await fixture.Client.PostAsJsonAsync(
                answersUrl, new AnswerRequest(item.Id, categoryId));

            Assert.Equal(HttpStatusCode.OK, answerResponse.StatusCode);

            var answer = await answerResponse.Content.ReadFromJsonAsync<AnswerResultDto>();
            Assert.NotNull(answer);
            Assert.Equal(isFinalAnswer, answer.IsGameCompleted);

            if (answer.IsCorrect)
            {
                expectedScore += 10;
            }

            var highscores = await fixture.Client
                .GetFromJsonAsync<List<HighscoreEntryDto>>(highscoresUrl);

            Assert.NotNull(highscores);

            if (isFinalAnswer)
            {
                var entry = Assert.Single(highscores, row => row.GuestNumber == game.GuestNumber);
                Assert.Equal(expectedScore, entry.Score);
                Assert.Equal("pindsvin", entry.AvatarKey);
            }
            else
            {
                Assert.DoesNotContain(highscores, row => row.GuestNumber == game.GuestNumber);
            }
        }
    }
}
