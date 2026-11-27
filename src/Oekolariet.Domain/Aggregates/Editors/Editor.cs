namespace Oekolariet.Domain.Aggregates.Editors;

// A staff member from Økolariet who can log in to the CMS and edit content
public sealed class Editor
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty; // never the password itself
    public DateTime CreatedAt { get; private set; }

    private Editor() { } // Only for EF Core

    // passwordHash must already be hashed. The Domain does not know how to hash.
    public static Editor Create(Guid id, string email, string passwordHash, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id må ikke være tomt.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email er påkrævet.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Adgangskode er påkrævet.", nameof(passwordHash));
        }

        return new Editor
        {
            Id = id,
            Email = email.Trim().ToLowerInvariant(), // same email, same editor, no matter the capital letters
            PasswordHash = passwordHash,
            CreatedAt = createdAtUtc,
        };
    }
}
