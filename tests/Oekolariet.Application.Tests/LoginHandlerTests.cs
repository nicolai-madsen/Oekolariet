using Moq;
using Oekolariet.Application.Auth;
using Oekolariet.Domain.Aggregates.Editors;
using Oekolariet.Shared.Auth;

namespace Oekolariet.Application.Tests;

// Unit tests for LoginHandler. The database, hasher and JWT are mocks (Moq),
// so we only test the login steps themselves.
public class LoginHandlerTests
{
    private const string Email = "admin@oekolariet.test";
    private const string Password = "Udvikling123!";
    private const string Hash = "saved-hash";

    // The mocks: fake versions of the three parts LoginHandler needs
    private readonly Mock<IEditorRepository> _editors = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IAccessTokenGenerator> _tokens = new();

    // One editor with a saved password hash
    private readonly Editor _editor = Editor.Create(Guid.NewGuid(), Email, Hash, DateTime.UtcNow);

    // The real LoginHandler, but with the mocks inside (.Object = the fake object)
    private LoginHandler CreateHandler()
    {
        return new LoginHandler(_editors.Object, _hasher.Object, _tokens.Object);
    }

    [Fact]
    public async Task HandleAsync_CorrectEmailAndPassword_ReturnsToken()
    {
        // Arrange: the editor exists, the password is correct, and the token generator returns a token
        var expected = new LoginResponseDto("a-token", DateTime.UtcNow);
        _editors.Setup(r => r.GetByEmailAsync(Email, It.IsAny<CancellationToken>())).ReturnsAsync(_editor);
        _hasher.Setup(h => h.Verify(Hash, Password)).Returns(true);
        _tokens.Setup(t => t.Create(_editor)).Returns(expected);

        // Act
        var result = await CreateHandler().HandleAsync(new LoginRequestDto(Email, Password), CancellationToken.None);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task HandleAsync_UnknownEmail_ReturnsNull()
    {
        // Arrange: no editor has this email
        _editors.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Editor?)null);

        // Act
        var result = await CreateHandler().HandleAsync(new LoginRequestDto("someone@else.test", Password), CancellationToken.None);

        // Assert: null (= 401), and no token was made
        Assert.Null(result);
        _tokens.Verify(t => t.Create(It.IsAny<Editor>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ReturnsNull()
    {
        // Arrange: the editor exists, but the password is wrong
        _editors.Setup(r => r.GetByEmailAsync(Email, It.IsAny<CancellationToken>())).ReturnsAsync(_editor);
        _hasher.Setup(h => h.Verify(Hash, "WrongPassword")).Returns(false);

        // Act
        var result = await CreateHandler().HandleAsync(new LoginRequestDto(Email, "WrongPassword"), CancellationToken.None);

        // Assert: same answer as an unknown email, and no token was made
        Assert.Null(result);
        _tokens.Verify(t => t.Create(It.IsAny<Editor>()), Times.Never);
    }

    [Theory]
    [InlineData("", Password)]   // no email
    [InlineData(Email, "")]      // no password
    [InlineData("   ", "   ")]   // only spaces
    public async Task HandleAsync_BlankEmailOrPassword_Throws(string email, string password)
    {
        // Act & Assert: ArgumentException (= 400)
        await Assert.ThrowsAsync<ArgumentException>(() => CreateHandler().HandleAsync(new LoginRequestDto(email, password), CancellationToken.None));
    }
}
