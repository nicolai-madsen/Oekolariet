using Oekolariet.Domain.Content;
using Oekolariet.Shared.Exhibitions;
using Oekolariet.Shared.Games;

namespace Oekolariet.Application.Common.Mapping;

internal static class ContentMappings
{
    // Maps content parts that multiple features share (draft, active version, game).
    // .this keyword is used to make the mapping methods extension methods,
    // so they can be called like theme.ToDto() instead of ContentMappings.ToDto(theme).
    public static ThemeDto ToDto(this Theme theme) =>
        new(theme.PrimaryColor, theme.SecondaryColor, theme.FontFamily, theme.LogoMediaId, theme.BackgroundMediaId);

    public static List<PageDto> ToDtos(this IReadOnlyList<Page> pages) =>
        pages
            .OrderBy(p => p.Order)
            .Select(p => new PageDto(p.Id, p.Order, p.Layout.ToString(), p.Title, p.Body, p.MediaId))
            .ToList();

    public static CategoryDto ToDto(this Category category) =>
        new(category.Id, category.Name, category.MediaId);
}
