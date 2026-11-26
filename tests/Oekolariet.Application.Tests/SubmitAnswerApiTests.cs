using System.Net;
using System.Net.Http.Json;
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
}
