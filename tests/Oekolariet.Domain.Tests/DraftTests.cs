using static Oekolariet.Domain.Tests.TestData;

namespace Oekolariet.Domain.Tests;

public class DraftTests
{
    [Fact]
    public void Create_WithValidInput_SetsAllProperties()
    {
        // Arrange
        var exhibitionId = Guid.NewGuid();
        var basedOnVersionId = Guid.NewGuid();
        var updatedBy = "test@dude.com";

        var content = CreateContent();
        var updatedAt = DateTime.UtcNow;


        // Act
        var draft = Draft.Create(exhibitionId, basedOnVersionId, content, updatedBy, updatedAt);

        // Assert
        Assert.Equal(updatedBy, draft.UpdatedBy);
        Assert.Equal(exhibitionId, draft.ExhibitionId);
        Assert.Equal(basedOnVersionId, draft.BasedOnVersionId);
        Assert.Equal(content, draft.Content);
        Assert.Equal(updatedAt, draft.UpdatedAt);
    }
}
