using Oekolariet.Domain.Aggregates.Editors;
using static Oekolariet.Domain.Tests.TestData; // shared test data, e.g. CreatedAt

namespace Oekolariet.Domain.Tests;

// Unit tests for Editor.Create: checks every rule an editor (admin) must follow
public class EditorTests
{
    // Any non-empty text works here. The Domain only stores the hash, it never checks how it was made.
    private const string Hash = "some-hashed-password";

    // The happy path: valid input gives an editor with all values set
    [Fact]
    public void Create_WithValidInput_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var editor = Editor.Create(id, "admin@oekolariet.test", Hash, CreatedAt);

        // Assert
        Assert.Equal(id, editor.Id);
        Assert.Equal("admin@oekolariet.test", editor.Email);
        Assert.Equal(Hash, editor.PasswordHash);
        Assert.Equal(CreatedAt, editor.CreatedAt);
    }

    // The same person can type the email with spaces or capital letters and still be the same editor
    [Fact]
    public void Create_TrimsAndLowercasesEmail()
    {
        // Arrange
        var email = "  Admin@Oekolariet.TEST  ";

        // Act
        var editor = Editor.Create(Guid.NewGuid(), email, Hash, CreatedAt);

        // Assert
        Assert.Equal("admin@oekolariet.test", editor.Email);
    }

    // An editor must have a real id
    [Fact]
    public void Create_WithEmptyId_Throws()
    {
        // Act & Assert: Create must throw before an editor is made
        Assert.Throws<ArgumentException>(() => Editor.Create(Guid.Empty, "admin@oekolariet.test", Hash, CreatedAt));
    }

    // An editor cannot log in without an email, so an empty, blank or missing email is not allowed
    [Theory]
    [InlineData("")]     // empty
    [InlineData("   ")]  // only spaces
    [InlineData(null)]   // missing
    public void Create_WithBlankEmail_Throws(string? blankEmail)
    {
        // Act & Assert ("!" tells the compiler we send null on purpose)
        Assert.Throws<ArgumentException>(() => Editor.Create(Guid.NewGuid(), blankEmail!, Hash, CreatedAt));
    }

    // An editor without a password hash could never log in, so it is not allowed
    [Theory]
    [InlineData("")]     // empty
    [InlineData("   ")]  // only spaces
    [InlineData(null)]   // missing
    public void Create_WithBlankPasswordHash_Throws(string? blankHash)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Editor.Create(Guid.NewGuid(), "admin@oekolariet.test", blankHash!, CreatedAt));
    }
}
