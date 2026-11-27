using Oekolariet.Domain.Content;
namespace Oekolariet.Domain.Tests;

public class TileTests
{
    [Fact]
    public void Create_WithValidValues_ReturnsExpected()
    {
        // Arrange
        var title = "Affaldshierarkiet";

        // Act
        var tile = new Tile(
            TileType.Hierarchy,
            title,
            MediaId: null);

        // Assert
        Assert.Equal(TileType.Hierarchy, tile.Type);
        Assert.Equal(title, tile.Title);
        Assert.Null(tile.MediaId);
    }
}

