namespace Oekolariet.Domain.Content;

public sealed record ExhibitionContent(Theme Theme, IReadOnlyList<Page> Pages)
{
    // Sets the standard theme for an exhibition, so it will always have a Theme
    public static ExhibitionContent Empty { get; } = new(
        new Theme("#FFFFFF", "#A5D6A7", "K2D", null, null),
        []);
}

public sealed record Theme(
    string PrimaryColor,
    string SecondaryColor,
    string FontFamily,
    Guid? LogoMediaId,
    Guid? BackgroundMediaId);

public sealed record Page(
    Guid Id,
    int Order,
    PageLayout Layout,
    string Title,
    string Body,
    Guid? MediaId);

public enum PageLayout
{
    ImageLeft,
    ImageRight,
    TextOnly,
    ImageOnly
}
