namespace Oekolariet.Domain.Content;

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
