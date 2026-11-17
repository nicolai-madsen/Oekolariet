using Oekolariet.Shared.Exhibitions;

namespace Oekolariet.Application.Exhibitions;

public sealed class GetActiveVersionHandler(IExhibitionRepository exhibitions, IExhibitionVersionRepository versions)
{
    public async Task<ActiveVersionDto?> HandleAsync(Guid exhibitionId, CancellationToken ct)
    {
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

        var theme = version.Content.Theme;

        return new ActiveVersionDto(
            exhibition.Id,
            version.Id,
            version.VersionNumber,
            version.PublishedAt,
            new ThemeDto(theme.PrimaryColor, theme.SecondaryColor, theme.FontFamily, theme.LogoMediaId, theme.BackgroundMediaId),
            version.Content.Pages
                .OrderBy(p => p.Order)
                .Select(p => new PageDto(p.Id, p.Order, p.Layout.ToString(), p.Title, p.Body, p.MediaId))
                .ToList());
    }
}
