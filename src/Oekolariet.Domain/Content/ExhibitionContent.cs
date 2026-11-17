namespace Oekolariet.Domain.Content;

public sealed record ExhibitionContent(Theme Theme, IReadOnlyList<Page> Pages);

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
