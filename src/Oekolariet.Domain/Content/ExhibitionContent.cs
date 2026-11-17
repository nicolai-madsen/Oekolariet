namespace Oekolariet.Domain.Content;

public sealed record ExhibitionContent(Theme Theme, IReadOnlyList<Page> Pages);

public sealed record Theme(
    string PrimaryColor,
    string SecondaryColor,
    string FontFamily,
    Guid? LogoMediaId,
    Guid? BackgroundMediaId);

public sealed record Page(
    Guid id,
    int Order,
    PageLayout Layout,
    string title,
    string Body,
    Guid? MediaId);

public enum PageLayout
{
    ImageLeft,
    ImageRight,
    TextOnly,
    ImageOnly
}
