using Oekolariet.Domain.Aggregates.Exhibitions;
using Oekolariet.Domain.Content;

namespace Oekolariet.Domain.Tests;

public class ExhibitionTests
{
    private static readonly DateTime CreatedAt = new(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);

    // Our glorious Object Mother: Valid content for the tests
    private static ExhibitionContent CreateContent() => new(
        new Theme("#ffffff", "#A5D6A7", "K2D", null, null),
        [new Page(Guid.NewGuid(), 1, PageLayout.TextOnly, "Velkommen!", "Jeg hedder Otto Odder", null)]);

    private static Exhibition CreateExhibition() => Exhibition.Create(Guid.NewGuid(), "TestExhibition", CreatedAt);

    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithBlankName_Throws(string? blankName)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Exhibition.Create(id, blankName!, CreatedAt));
    }

    [Fact]
    public void Create_TrimsName_ReturnsExpected()
    {
        // Arrange
        var name = "    testguy    ";
        var id = Guid.NewGuid();

        // Act
        var exhibition = Exhibition.Create(id, name, CreatedAt);

        // Assert
        Assert.Equal("testguy", exhibition.Name);
        Assert.Equal(id, exhibition.Id);
        Assert.Equal(CreatedAt, exhibition.CreatedAt);
        Assert.False(exhibition.IsArchived);
        Assert.Null(exhibition.ActiveVersionId);
    }

    [Fact]
    public void Publish_FirstTime_CreatesVersion1AndMakesItActive()
    {
        // Arrange
        var exhibition = CreateExhibition();

        // Act
        var version = exhibition.Publish(CreateContent(), "test@example.com", CreatedAt);

        // Assert
        Assert.Equal(1, version.VersionNumber);
        Assert.Equal(exhibition.Id, version.ExhibitionId);
        Assert.Equal(version.Id, exhibition.ActiveVersionId);
    }

    [Fact]
    public void Publish_Twice_IncrementsVersionNumberAndMovesActivePointer()
    {
        // Arrange
        var exhibition = CreateExhibition();
        var first = exhibition.Publish(CreateContent(), "test@example.com", CreatedAt);

        // Act
        var second = exhibition.Publish(CreateContent(), "test@example.com", CreatedAt.AddHours(1));

        // Assert
        Assert.Equal(2, second.VersionNumber);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(second.Id, exhibition.ActiveVersionId);
    }
}
