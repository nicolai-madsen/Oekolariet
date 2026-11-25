using Oekolariet.Shared.Auth;

namespace Oekolariet.Application.Auth;

// The login use case: checks the email and password, and gives back a token if they are correct
public sealed class LoginHandler(
    IEditorRepository editors,
    IPasswordHasher passwordHasher,
    IAccessTokenGenerator tokenGenerator)
{
    // Returns a token, or null if the email or password is wrong (the endpoint turns null into 401)
    public async Task<LoginResponseDto?> HandleAsync(LoginRequestDto request, CancellationToken ct)
    {
        // Both fields are required (an ArgumentException becomes 400 in the API)
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email og adgangskode er påkrævet.");
        }

        // 1. Find the editor with this email
        var editor = await editors.GetByEmailAsync(request.Email, ct);
        if (editor is null)
        {
            return null; // 2. No editor with this email
        }

        // 3. Check the password against the saved hash
        if (!passwordHasher.Verify(editor.PasswordHash, request.Password))
        {
            return null; // Wrong password
        }

        // 4. Correct: create the token
        return tokenGenerator.Create(editor);
    }
}
