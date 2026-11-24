using Oekolariet.Domain.Content;

namespace Oekolariet.Domain.Tests;

public class SortinGameTests
{
    private static readonly Category Plast = new(Guid.NewGuid(), "Plast", null);
    private static readonly Category Pap = new(Guid.NewGuid(), "Pap", null);

    private static SortingGame CreateGame(params SortingItem[] items) =>
        new("Sortér affaldet", "Træk tingene over i den rigtige spand", 30, [Plast, Pap], items);

    [Fact]
    public void ValidateForPublish_WithValidGame_ReturnsNoErrors()
    {
        // Arrange
        var game = CreateGame(
            new SortingItem(Guid.NewGuid(), "Mælkekarton", null, Pap.Id, null),
            new SortingItem(Guid.NewGuid(), "Flaske", null, Plast.Id, null));

        // Act
        var errors = game.ValidateForPublish();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateForPublish_WithNoItems_ReturnOneError()
    {
        // Arrange
        var game = CreateGame();

        // Act
        var errors = game.ValidateForPublish();

        // Assert
        Assert.Single(errors);
    }

    [Fact]
    public void ValidateForPublish_WithItemPointingToMissingCategory_ReturnsErrorThatItemOnly()
    {
        // Arrange
        var game = CreateGame(
            new SortingItem(Guid.NewGuid(), "Mælkekarton", null, Pap.Id, null),
            new SortingItem(Guid.NewGuid(), "Flaske", null, Guid.NewGuid(), null));

        // Act
        var errors = game.ValidateForPublish();

        // Assert
        var error = Assert.Single(errors);
        Assert.Contains("Flaske", error);
    }
}
