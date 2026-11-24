namespace Oekolariet.Domain.Content;

public sealed record ExhibitionContent(Theme Theme, IReadOnlyList<Page> Pages, SortingGame Game)
{
    // Sets the standard theme for an exhibition, so it will always have a Theme
    public static ExhibitionContent Empty { get; } = new(
        new Theme("#FFFFFF", "#A5D6A7", "K2D", null, null),
        [],
        SortingGame.Empty);
}

public sealed record SortingGame(
    string Title,
    string Intro,
    int DurationSeconds,
    IReadOnlyList<Category> Categories,
    IReadOnlyList<SortingItem> Items)
{
    public static SortingGame Empty { get; } = new("", "", 30, [], []);

    public IReadOnlyList<string> ValidateForPublish()
    {
        var errors = new List<string>();

        if (Items.Count == 0)
        {
            errors.Add("Spillet har ingen genstande.");
        }

        var categoryIds = Categories.Select(c => c.Id).ToHashSet();

        foreach (var item in Items.Where(i => !categoryIds.Contains(i.CorrectCategoryId)))
        {
            errors.Add($"'{item.Name}' peger på en kategori, der ikke findes.");
        }

        return errors;
    }
}

public sealed record Category(Guid Id, string Name, Guid? MediaId);

public sealed record SortingItem(
    Guid Id,
    string Name,
    Guid? MediaId,
    Guid CorrectCategoryId,
    string? Explanation);

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
