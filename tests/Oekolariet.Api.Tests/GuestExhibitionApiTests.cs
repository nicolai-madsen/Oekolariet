using System.Net;
using System.Net.Http.Json;
using Oekolariet.Shared.Exhibitions;

namespace Oekolariet.Api.Tests;

// Run with Docker running. These tests call the guest endpoint anonymously
// and verify that guests can retrieve only published exhibition versions.
[Collection("ApiTests")]
public sealed class GuestExhibitionApiTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task GetActiveVersion_ForPublishedExhibition_ReturnsItsVersion()
    {
        // The development seed creates this exhibition with an active version.
        var exhibitionId = Guid.Parse("9b1d5e2a-0c4f-4e7a-9a51-3f1c0e8b0001");
        var path = $"/api/v1/exhibitions/{exhibitionId}/active-version";

        using var response = await fixture.Client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var version = await response.Content.ReadFromJsonAsync<ActiveVersionDto>();
        Assert.NotNull(version);
        Assert.Equal(exhibitionId, version.ExhibitionId);
        Assert.NotEqual(Guid.Empty, version.VersionId);
    }

    [Fact]
    public async Task GetActiveVersion_ForUnknownExhibition_ReturnsNotFound()
    {
        var path = $"/api/v1/exhibitions/{Guid.NewGuid()}/active-version";

        using var response = await fixture.Client.GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveVersion_ForUnpublishedExhibition_ReturnsNotFound()
    {
        var exhibitionId = await fixture.CreateUnpublishedExhibitionAsync();
        var path = $"/api/v1/exhibitions/{exhibitionId}/active-version";

        using var response = await fixture.Client.GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
