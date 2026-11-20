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
        var updatedAt = CreatedAt;


        // Act
        var draft = Draft.Create(exhibitionId, basedOnVersionId, content, updatedBy, updatedAt);

        // Assert
        Assert.Equal(updatedBy, draft.UpdatedBy);
        Assert.Equal(exhibitionId, draft.ExhibitionId);
        Assert.Equal(basedOnVersionId, draft.BasedOnVersionId);
        Assert.Equal(content, draft.Content);
        Assert.Equal(updatedAt, draft.UpdatedAt);
    }

    [Fact]
    public void Create_WithEmptyExhibitionId_Throws()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var basedOnVersionId = Guid.NewGuid();
        var content = CreateContent();
        var updatedBy = "test@dude.com";
        var updatedAt = CreatedAt;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Draft.Create(emptyGuid, basedOnVersionId, content, updatedBy, updatedAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithBlankUpdatedBy_Throws(string? updatedBy)
    {
        // Arrange
        var exhibitionId = Guid.NewGuid();
        var basedOnVersionId = Guid.NewGuid();
        var content = CreateContent();
        var updatedAt = CreatedAt;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Draft.Create(exhibitionId, basedOnVersionId, content, updatedBy!, updatedAt));
    }

    [Fact]
    public void Replace_UpdatesContentAndAuditFields_ButKeepsBasedOnVersionId()
    {
        // Arrange
        var basedOnVersionId = Guid.NewGuid();
        var draft = Draft.Create(Guid.NewGuid(), basedOnVersionId, CreateContent(), "testTorben@employee.com", CreatedAt);

        var newContent = CreateContent();
        var newUpdatedBy = "testTine@employee.com";
        var newUpdatedAt = CreatedAt.AddHours(1);

        // Act
        draft.Replace(newContent, newUpdatedBy, newUpdatedAt);

        // Assert
        Assert.Same(newContent, draft.Content);
        Assert.Equal("testTine@employee.com", draft.UpdatedBy);
        Assert.Equal(newUpdatedAt, draft.UpdatedAt);

        Assert.Equal(basedOnVersionId, draft.BasedOnVersionId);
    }
}
