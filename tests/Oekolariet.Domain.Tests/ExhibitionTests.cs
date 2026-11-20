using Oekolariet.Domain.Aggregates.Exhibitions;
using static Oekolariet.Domain.Tests.TestData;
namespace Oekolariet.Domain.Tests;

public class ExhibitionTests
{
    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithBlankName_Throws(string? blankName)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Exhibition.Create(id, blankName!, CreatedAt)); // CreatedAt data comes from TestData class
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
        var version = exhibition.Publish(TestData.CreateContent(), "test@example.com", CreatedAt);

        // Assert
        Assert.Equal(1, version.VersionNumber);
        Assert.Equal(exhibition.Id, version.ExhibitionId);
        Assert.Equal(version.Id, exhibition.ActiveVersionId);
    }

    [Fact]
    public void Publish_Twice_IncrementsVersionNumberAndMovesActivePointer()
    {
        // Arrange
        var exhibition = TestData.CreateExhibition();
        var first = exhibition.Publish(TestData.CreateContent(), "test@example.com", CreatedAt);

        // Act
        var second = exhibition.Publish(TestData.CreateContent(), "test@example.com", CreatedAt.AddHours(1));

        // Assert
        Assert.Equal(2, second.VersionNumber);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(second.Id, exhibition.ActiveVersionId);
    }
}
