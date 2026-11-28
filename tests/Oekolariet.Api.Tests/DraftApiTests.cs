using System.Net;
using System.Net.Http.Json;
using Oekolariet.Shared.Drafts;

namespace Oekolariet.Api.Tests;

// Run with Docker running. These tests call the draft endpoint through HTTP.
// Each test that creates a draft uses its own exhibition in the shared SQL Server database.
[Collection("ApiTests")]
public sealed class DraftApiTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task CreateDraft_ForUnpublishedExhibition_ReturnsCreatedWithLocation()
    {
        var exhibitionId = await fixture.CreateUnpublishedExhibitionAsync();
        var path = $"/api/v1/exhibitions/{exhibitionId}/draft";

        using var response = await fixture.EditorClient.PostAsync(path, content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(path, response.Headers.Location?.OriginalString);

        var draft = await response.Content.ReadFromJsonAsync<DraftDto>();
        Assert.NotNull(draft);
        Assert.Equal(exhibitionId, draft.ExhibitionId);
        Assert.Equal("admin@oekolariet.test", draft.UpdatedBy); // the email comes from the token
    }

    [Fact]
    public async Task CreateDraft_ForUnknownExhibition_ReturnsNotFound()
    {
        var path = $"/api/v1/exhibitions/{Guid.NewGuid()}/draft";

        using var response = await fixture.EditorClient.PostAsync(path, content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateDraft_WhenDraftAlreadyExists_ReturnsConflict()
    {
        var exhibitionId = await fixture.CreateUnpublishedExhibitionAsync();
        var path = $"/api/v1/exhibitions/{exhibitionId}/draft";

        // Create the first draft here so the conflict test can run on its own.
        using var firstResponse = await fixture.EditorClient.PostAsync(path, content: null);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var secondResponse = await fixture.EditorClient.PostAsync(path, content: null);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task CreateDraft_WithoutLogin_ReturnsUnauthorized()
    {
        var exhibitionId = await fixture.CreateUnpublishedExhibitionAsync();
        var path = $"/api/v1/exhibitions/{exhibitionId}/draft";

        // Client has no token, so default deny must stop it (AC-02)
        using var response = await fixture.Client.PostAsync(path, content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
