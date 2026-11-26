namespace Oekolariet.Domain.Content;

public sealed record ExhibitionContent(Theme Theme, IReadOnlyList<Page> Pages, IReadOnlyList<Tile> Tiles, SortingGame Game)
{
    // Sets the standard theme for an exhibition, so it will always have a Theme
    public static ExhibitionContent Empty { get; } = new(
        new Theme("#FFFFFF", "#A5D6A7", "K2D", null, null),
        [],
        [],
        SortingGame.Empty);
}
