using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Domain.Content;

namespace Oekolariet.Domain.Tests;

internal static class TestData
{
    public static readonly DateTime CreatedAt = new(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);

    // Our glorious Object Mother: Valid content for the tests
    public static ExhibitionContent CreateContent() => new(
        new Theme("#ffffff", "#A5D6A7", "K2D", null, null),
        [new Page(Guid.NewGuid(), 1, PageLayout.TextOnly, "Velkommen!", "Jeg hedder Otto Odder", null)],
        [],
        SortingGame.Empty);

    public static Exhibition CreateExhibition() => Exhibition.Create(Guid.NewGuid(), "TestExhibition", CreatedAt);
}
