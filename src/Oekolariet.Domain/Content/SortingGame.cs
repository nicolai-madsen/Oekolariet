namespace Oekolariet.Domain.Content;

public sealed record SortingGame(
    string Title,
    string Intro,
    int DurationSeconds,
    IReadOnlyList<Category> Categories,
    IReadOnlyList<SortingItem> Items)
{
    // Valid during a draft, but cannot be published (because of no items)
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
