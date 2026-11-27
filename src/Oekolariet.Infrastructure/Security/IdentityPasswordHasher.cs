using Microsoft.AspNetCore.Identity;
using Oekolariet.Application.Auth;
using Oekolariet.Domain.Aggregates.Editors;

namespace Oekolariet.Infrastructure.Security;

// Uses ASP.NET Core's PasswordHasher (PBKDF2 with a random salt) to keep IPasswordHasher's promise
internal sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<Editor> _hasher = new();

    public string Hash(string password)
    {
        // The built-in hasher does not use the editor, so we send null on purpose ("!")
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string passwordHash, string password)
    {
        var result = _hasher.VerifyHashedPassword(null!, passwordHash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
