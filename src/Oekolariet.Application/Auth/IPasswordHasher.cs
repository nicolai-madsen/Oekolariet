namespace Oekolariet.Application.Auth;

// Turns a password into a hash, and checks a password against a saved hash.
// Application only knows this promise. Infrastructure decides how it is done.
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}
