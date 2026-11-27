using Oekolariet.Application.Common.Mapping;
using Oekolariet.Shared.Exhibitions;

namespace Oekolariet.Application.Exhibitions;

//Contains the GetActiveVersionHandler class, which is responsible for retrieving the active version of an exhibition.
public sealed class GetActiveVersionHandler(IExhibitionRepository exhibitions, IExhibitionVersionRepository versions)
{
    public async Task<ActiveVersionDto?> HandleAsync(Guid exhibitionId, CancellationToken ct)
    {
        // Perform a series of checks to ensure that the exhibition and its active version exist and are valid.
        var exhibition = await exhibitions.GetByIdAsync(exhibitionId, ct);

        if (exhibition is null || exhibition.IsArchived || exhibition.ActiveVersionId is not Guid versionId)
        {
            return null;
        }

        var version = await versions.GetByIdAsync(versionId, ct);

        if (version is null)
        {
            return null;
        }
        // Extract the theme from the version's content for use in the ActiveVersionDto.
        var theme = version.Content.Theme;

        // Once all the preconditions are met, we can return the ActiveVersionDto with the relevant data.
        return new ActiveVersionDto(
            exhibition.Id,
            version.Id,
            version.VersionNumber,
            version.PublishedAt,
            version.Content.Theme.ToDto(),
            version.Content.Pages.ToDtos(),
            version.Content.Tiles.ToDtos()
            );
    }
}
